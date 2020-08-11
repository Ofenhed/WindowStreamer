using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace WindowStreamer
{
    class MjpegStreamer
    {
        private String boundary;
        private bool die;
        private int port;

        private BroadcastBlock<Lazy<byte[]>> currentImageEncoded;
        private Bitmap currentImage;
        private Thread serverThread = null;
        private HttpListener host;
        private ImageCodecInfo imageCodecInfo;

        public MjpegStreamer(string boundary, int? port)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == ImageFormat.Jpeg.Guid)
                {
                    imageCodecInfo = codec;
                    break;
                }
            }

            this.boundary = boundary;
            this.port = port.Value;
            currentImageEncoded = new BroadcastBlock<Lazy<byte[]>>(null);
            die = false;
        }


        public void Run(Action<Uri> callback)
        {
            host = new HttpListener();
            try
            {
                var stream_num = 0;
                while (true)
                {
                    try
                    {
                        var url = new UriBuilder()
                        {
                            Scheme = "http",
                            Host = "*",
                            Port = port,
                            Path = $"/stream/{stream_num}/"
                        };
                        host.Prefixes.Add(url.ToString());
                        host.Start();
                        url.Host = "127.0.0.1";
                        callback(url.Uri);
                        break;
                    }
                    catch
                    {
                        host = new HttpListener();
                        ++stream_num;
                    }
                }
                while (!die)
                {
                    var context = host.GetContext();
                    var request = context.Request;
                    var response = context.Response;
                    response.Headers.Add($"Content-Type:multipart/x-mixed-replace; boundary={boundary}");
                    response.StatusCode = 200;
                    new Thread(delegate () { streamCallback(response.OutputStream); }) { Priority = ThreadPriority.BelowNormal, IsBackground = true }.Start();
                }
            }
            finally
            {
                if (host.IsListening)
                {
                    host.Prefixes.Clear();
                    host.Stop();
                    host.Close();
                }
            }
        }

        public void Start(Action<Uri> callback)
        {
            if (serverThread == null)
            {
                serverThread = new Thread(delegate () { Run(callback); }) { Priority = ThreadPriority.Lowest, IsBackground = true };
                serverThread.Start();
            }
        }

        public void Stop()
        {
            die = true;
            if (serverThread == null)
            {
                return;
            }
            if (serverThread.IsAlive)
            {
                serverThread.Abort();
            }
            if (host != null)
            {
                host.Abort();
            }
        }

        private static void writeString(Stream stream, String text) // This should be replaced with the release of .NET 5
        {
            var encoded = Encoding.ASCII.GetBytes(text);
            stream.Write(encoded, 0, encoded.Length);
        }

        private void writeHeader(Stream stream, int length)
        {
            writeString(stream, "--");
            writeString(stream, boundary);
            writeString(stream, $"\r\nContent-Type:image/jpeg\r\nContent-Length:{length}\r\n\r\n");
        }

        private void writeFooter(Stream stream)
        {
            writeString(stream, "\r\n");
        }

        private byte[] encodeImage(EncoderParameters encoderParameters)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var myImage = Interlocked.Exchange(ref currentImage, null);
                if (myImage == null)
                    return null;
                myImage.Save(ms, imageCodecInfo, encoderParameters);
                var maybeNewImage = Interlocked.Exchange(ref currentImage, myImage);
                // This tries to recycle the old bitmap. If a new bitmap has
                // already been placed we'll simply drop it, since the 
                // alternative is ta draw that frame instead, but that
                // may lead to starvation, since the producer may be
                // faster than the consumer every frame.
                if (maybeNewImage != null)
                    maybeNewImage.Dispose();
                return ms.ToArray();
            }
        }

        public Bitmap SetCurrentImage(Bitmap image, EncoderParameters encoderParameters)
        {
            var lastImage = Interlocked.Exchange(ref currentImage, image);

            currentImageEncoded.SendAsync(new Lazy<byte[]>(() => encodeImage(encoderParameters)));

            return lastImage;
        }

        class BroadcastObserver<T> : System.IObserver<T>
        {
            private Semaphore onReadySem;
            private Action<T> onDataAction;

            public BroadcastObserver(Semaphore ready, Action<T> onData)
            {
                onReadySem = ready;
                onDataAction = onData;
            }

            public void OnCompleted()
            {
                onReadySem.Release();
            }

            public void OnError(Exception error)
            {
                onReadySem.Release();
            }

            public void OnNext(T value)
            {
                onDataAction(value);
            }
        }

        private void streamCallback(Stream stream)
        {
            Semaphore semaphore = new Semaphore(0, 1);
            Action<Lazy<byte[]>> handler = imageData =>
            {
                try
                {
                    if (imageData.Value != null)
                    {
                        var value = imageData.Value;
                        writeHeader(stream, value.Length);
                        stream.Write(value, 0, value.Length);
                        writeFooter(stream);
                        stream.Flush();
                    }
                }
                catch (HttpListenerException) { }
                catch (ProtocolViolationException)
                {
                    semaphore.Release();
                }
            };
            var unsubscriber = currentImageEncoded.AsObservable().Subscribe(new BroadcastObserver<Lazy<byte[]>>(semaphore, handler));
            semaphore.WaitOne();
            unsubscriber.Dispose();
            stream.Close();
        }

    }

}
