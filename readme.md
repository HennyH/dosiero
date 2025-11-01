# Dosiero

That's esperanto for file. This program makes it easy to monetize your files with Monero (XMR).

You point the program to a directory using the `--folder` parameter, and it will search for `index.toml` files.

```
/store
    .dosiero
    /music
        /My First Album

            01 - Track 1.mp3
            02 - Track 2.mp3
```

```toml
[[files]]
name = My First Album
path = ./music/My First Album
price = 0.004 XMR

[[files]]
name = 01 - Track 1.mp3
path = ./music/My First Album/01 - Track 1.mp3
price = 0.0005 XMR
```

You can browse the directory by visiting `/browse`. When a file is selected you will be taken to `/buy?file=<uuid>`, which will then perform a temporary re-direct to 
```
/buy?
    file=<uuid>
    &to=monero:<address>?tx_amount=<amount>&tx_description=<name>
    &expiry=<date>
    &hash=<hash>
    [&proof=<txproof>]
```
. The payment screen will then be displayed.

1. If the `hash` does not equal `hash(to)` an error will be displayed.
2. If payment has been made to `to` in the correct amount a download link will appear as `/download?hash=aK3b...&to=monero:84...84?tx_amount=0.0005&tx_description=My First Album`, clicking this link will allow access to the file.
3. Otherwise, a QR code is displayed for payment. This page is automatically refreshed every 10s whilst waiting for the payment to be received.
4. In all cases a button to bookmark the URL is displayed and both a draggable link `<a href="current-page-url" title="..." />` and `"Press Ctrl+D (Windows/Linux) or Cmd+D (Mac) to bookmark this page.`.

On start up I scan for all .dosiero files,
I start a file watcher for .dosiero files
All parsed files go into a list of nodes by depth
[
    [N_a, N_bb]
    [N_aa, N_ab, ...]
    ...
]

Then to find the relevant node you search from the lowest depth, these results can be cached

(Max(Node Timestamp), Path) => Node?

/files/...
/buy?