(function ($) {
    $(document).ready(function () {
        initialTimepicker();
    });

}(window.jQuery));

// Инициализирует поля ввода времени.
function initialTimepicker() {
    $('.timepicker').timepicker({
        timeOnlyTitle: 'Выберите время',
        timeText: 'Время',
        hourText: 'Часы',
        minuteText: 'Минуты',
        secondText: 'Секунды',
        currentText: 'Сейчас',
        closeText: 'Закрыть'
    });
}

// Выполняется при успешной отправке формы изменения заказа.
// Если сервер возвращает "Redirect", обновляет страницу в соответствии с переданным url.
// В противном случае заменяет исходную строку таблицы на ответ сервера.
function onEditOrder(res) {
    if (res.result == 'Redirect') {
        window.location = res.url;
        return;
    }
    else {
        var selector = $(res).attr('id');
        $('#' + selector + '-validation').remove();
        $('#' + selector).replaceWith(res);
        initialTimepicker();
    }
}