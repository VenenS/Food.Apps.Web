(function ($) {
    $(document).ready(function () {

        $(".cancel-order").on('click', function (e) {
            e.stopPropagation();
            var source = $(this);
            var link = source.data('url');
            var updateTarget = source.data("update-target");
            var target = $("#approveBtn");
            target.attr('data-url', link);
            target.attr('data-update-target', updateTarget);
            $('#approveModal').modal();
        });
        
        expandOrder();
        
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
)
    

}(window.jQuery));

function approveAction(btn) {
    var url = $(btn).data('url');
    var updateTarget = $(btn).data('update-target');
    $.ajax({
        type: "POST",
        url: url,
        traditional: true,
        success: function (data) {
            $(updateTarget).replaceWith(data);
        },
        complete: function () {
            $('#approveModal').modal('hide');
        }
    });
}

function expandOrder() {
    $('.expand-order').on('click', function () {
        var elem = $(this);
        var load = elem.attr('data-load');
        if (load) {
            var id = elem.parent().parent().data('item-id');
            var url = '/curator/reports/userorders?companyorderid=' + id;
            var target = elem.data('target');
            $.ajax({
                url: url,
                success: function (data) {
                    $(target).html(data);
                    elem.removeAttr('data-load');
                    expandOrderItems(target);
                }
            })
        }
    })
}

function expandOrderItems(target) {
    $(target).on('click', '.expand-order-items', function () {
        var elem = $(this);
        var load = elem.attr('data-load');
        if (load) {
            var id = elem.parent().parent().data('item-id');
            $.ajax({
                url: '/curator/reports/orderitems?orderid=' + id,
                success: function (data) {
                    $('#order-positions-' + id).html(data);
                    elem.removeAttr('data-load')
                    elem.collapse();
                }
            })
        }
    })
}

//Показать дропдаун сотрудников в "Файл отчета"
function ShowRadioButtonPrintFileReport() {
    $('#employeeNames').removeClass('hidden');
}

function HiddenRadioButtonFileReport() {
    $('#employeeNames').addClass('hidden');
}