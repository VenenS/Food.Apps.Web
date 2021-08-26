(function ($) {
    $(document).ready(function () {
    });

}(window.jQuery));

function scrollToAddedCafe() {
    var centerScreen = $(window).height() / 2;
    var target = $('#listCafes li:last');
    $('html, body').animate({
        scrollTop: target.offset().top - centerScreen,
    }, 500);

    $(target).find('.collapse').collapse();
    $(target).focus();
}
function scrollToAddedLayout() {
    var centerScreen = $(window).height() / 2;
    var target = $('#LayoutsList li:last');
    $('html, body').animate({
        scrollTop: target.offset().top - centerScreen,
    }, 500);

    $(target).focus();
}

function deleteCafe(btn) {
    var url = $(btn).attr('data-url');
    var updateTarget = $(btn).attr('data-update-target');
    $.ajax({
        type: "POST",
        url: url,
        traditional: true,
        complete: function () {
            $('#deleteCafeModal').modal('hide');
            $(updateTarget).remove();
        }
    });
}

function addCafe(res) {
    $('#listCafes').append(res);
}

function onSuccessAddCafe(data) {
    addCafe(data);
    scrollToAddedCafe();
}

function saveCafe(res) {
    var target = '#' + $(this).parents('.list-group-item').attr('id');
    saveListGroupItem(res, target);
}

function expandOrderItems(target) {
    $(target).on('click', '.expand-order-items', function () {
        var elem = $(this);
        var load = elem.attr('data-load');
        if (load) {
            var id = elem.parent().parent().data('item-id');
            $.ajax({
                url: '/reports/orderitems?orderid=' + id,
                success: function (data) {
                    $('#order-positions-' + id).html(data);
                    elem.removeAttr('data-load')
                    elem.collapse();
                }
            })
        }
    })
}

function expandOrders() {
    $('.expand-order').on('click', function (e) {
        var elem = $(this);
        var load = elem.attr('data-load');
        if (load) {
            var id = elem.parent().parent().data('item-id');
            var url = '/reports/userorders?companyorderId=' + id;
            var target = elem.data('target');
            if (!id) {
                id = elem.parent().parent().data('banket-id');
                url = '/reports/banketorders?banketId=' + id;
                target = target + ' td'
            }
            $.ajax({
                url: url,
                success: function (data) {
                    $(target).html(data);
                    elem.removeAttr('data-load');
                }
            })
        }
    })
}

function saveListGroupItem(res, target) {
    var isOpen = $(target).find('.collapse').hasClass('in');
    $(target).replaceWith(res);
    if (isOpen) {
        $('#' + $(res).attr('id')).find('.collapse').addClass('in');
    }
}

function saveCategory(res) {
    saveAction(res, '#' + $(this).parents('.list-group-item').attr('id'));
}

function saveLayout(res) {
    saveAction(res, '#' + $(this).parents('.list-group-item').attr('id'));
}

function saveAction(res, target) {
    if (res.result == 'Redirect') {
        window.location = res.url;
        return;
    }
    saveListGroupItem(res, target);
}