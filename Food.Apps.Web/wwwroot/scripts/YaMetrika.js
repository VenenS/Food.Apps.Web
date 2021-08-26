(function (doc, win, callback) {
    (win[callback] = win[callback] || []).push(function () {
        try {
            win.yaCounter35784970 = new Ya.Metrika({
                id: 35784970,
                clickmap: true,
                trackLinks: true,
                accurateTrackBounce: true,
                webvisor: true
            });
        } catch (e) { }
    });

    var n = doc.getElementsByTagName("script")[0],
        s = doc.createElement("script"),
        f = function () { n.parentNode.insertBefore(s, n); };
    s.type = "text/javascript";
    s.async = true;
    s.src = "https://mc.yandex.ru/metrika/watch.js";

    if (win.opera == "[object Opera]") {
        doc.addEventListener("DOMContentLoaded", f, false);
    } else { f(); }
})(document, window, "yandex_metrika_callbacks");