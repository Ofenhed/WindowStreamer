# WindowStreamer

This project enables users to move view only windows, such as Discord Pop out window, to another computer.

Currently, it's locked to port 8181, which has to be opened for the current user (from Admin):

```
netsh http add urlacl url=http://*:8181/ user=YOURDOMAIN\YOURUSER
```

Notice that the port also has to be opened in the firewall.

Once this is done, open the program and draw from the empty program area to the window you want to send to the other computer. This will enable size, max fps and jpeg quality controls. The stream will be available on http://127.0.0.1:8181/. Viewing it from additional computers does not significantly affect performance.

To connect to the stream from linux, the following command can be used for great performance:

```
mpv --untimed http://streamserver:8181/ --profile=low-latency
```

## Known issues

* The server runs on port 8181 with no fallback, and admin access is needed to allow users to open these ports (albeit not to actually run the program). It would be better if it used an unprivileged HTTP server.
* The program continously takes screenshots. It would be better if the program could hook the WM_PAINT message.
* The background processes don't correctly die, so a ghost process will be running in the background when the program is shut down.