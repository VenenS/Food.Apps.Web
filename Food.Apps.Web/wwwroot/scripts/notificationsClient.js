(function ($) {
    $(document).ready(function () {
        notifications();
    })
}(window.jQuery));




function notifications() {
    var hub = $

        .notificationsHub;

    hub.client.sendMessage = function (title, message, action, icon) {
        var msg = {
            title: title,
            text: message,
            image: "<i class='fa fa-3x fa-" + icon + "-circle' > </i>"
        };

        $.notify(msg, action);
        titleNotify.begin('Новое сообщение');
    };
        
    hub.client.redirectTo = function (target) {
        window.location = target;
    };

    hub.client.newOrders = function (count) {
        var msg = {
            title: title,
            text: 'На данный момент необработанных заказов - ' + count,
            image: "<i class='fa fa-3x fa-info-circle' > </i>"
        };

        $.notify(count, { autoHide: false });
        titleNotify('Новое сообщение');
    };

    $.connection.hub.start()
        .done(function () { console.log('Now connected, connection ID=' + $.connection.hub.id); })
        .fail(function () { console.log('Could not Connect!'); });
};


var titleNotify = {
    options: {
        originalTitle: document.title,
        interval: null,
        timeout: null
    },

    begin: function (notification, intervalSpeed, timeoutDelay) {
        var _this = this;
        if (!document.hasFocus()) {
            blink(notification, intervalSpeed, timeoutDelay);
        }

        function blink(notification, intervalSpeed, timeoutDelay) {
            stop();
            _this.options.interval = setInterval(changeTitle, (intervalSpeed) ? intervalSpeed : 1000);
            _this.options.timeout = setTimeout(stop, (timeoutDelay) ? timeoutDelay : 10000);
        };

        function changeTitle() {
            document.title = (_this.options.originalTitle === document.title)
                        ? notification
                        : _this.options.originalTitle;
        }

        document.onfocus = stop;

        function stop() {
            clearInterval(_this.options.interval);
            clearTimeout(_this.options.timeout);
            document.title = _this.options.originalTitle;
        }
    }
}

