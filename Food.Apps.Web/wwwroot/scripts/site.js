(function ($) {
    $(document).ready(function () {
        initEvents();
        initDatePickers();
        //configureNotify();

        $('.scrollbar-inner').scrollbar();
        $('#feedbackPanelForm #Url').val(location.href);
        $('.delivery-order-time option:selected').each(function (index, el) {
            hiddenDropDownListByChild(el, $(el).parent('select').attr('id'));
        });
    }).ajaxComplete(function (event, xhr, settings) {
        $('.scrollbar-inner').scrollbar();
        initDatePickers();
    });

    $(window).bind('load', function () {
        $('img').each(function () {
            if ((typeof this.naturalWidth !== "undefined" &&
                this.naturalWidth === 0)
                || this.readyState === 'uninitialized') {
                $(this).remove();
            }
        });
    });

    jQuery.validator.addMethod("notEqual", function (value, element, param) {
        return this.optional(element) || value != $('#' + param).val();
    }, "");
    
}(window.jQuery));

function initEvents() {
    
    //Показывает и скрывает кнопку сборса в поле для поиска
    $('#filter').on('input', function () {

        if ($('#filter').val() !== '') {
            $('.btn-reset').addClass('is-type');
            $('.btn-search').addClass('is-type');
        }

        else {
            $('.btn-reset').removeClass('is-type');
            $('.btn-search').removeClass('is-type');
        }
        ;
    });
    
    var _gotoToday = jQuery.datepicker._gotoToday;
    jQuery.datepicker._gotoToday = function (a) {
        var target = jQuery(a);
        var inst = this._getInst(target[0]);
        _gotoToday.call(this, a);
        jQuery.datepicker._selectDate(a, jQuery.datepicker._formatDate(inst, inst.selectedDay, inst.selectedMonth, inst.selectedYear));
    };

    $('body').prepend('<a href="#" class="back-to-top"><i class="fa fa-arrow-up"></i></a>');

    $('a.back-to-top').click(function () {
        $('html, body').animate({
            scrollTop: 0
        }, 500);
        return false;
    });

    var amountScrolled = 300;
    $(window).scroll(function () {
        if ($(window).scrollTop() > amountScrolled) {
            $('a.back-to-top').fadeIn('slow');
        } else {
            $('a.back-to-top').fadeOut('slow');
        }
    });

    $('[data-toggle="popover"]').popover();
    
    $(document).on('click', '.btn-copy', function () {
        var target = $(this).siblings('.copy-text')[0];
        target.select();
        document.execCommand('copy');
    });

    $(document).on('click', '.btn-reset', function () {
        var form = $(this).closest('form');
        form.find('input[type="search"]').val('');
        $('span.btn-reset').removeClass('is-type');
        $('button.btn-search').removeClass('is-type');
        form.submit();
    });

    $(document).on('click', '.btn-toggle-groups', function (e) {
        var target = $($(this).data('target'));
        if (!target)
            return;
        var newstate = $(this).attr('state') ^ 1,
            icon = newstate ? "minus" : "plus",
            text = newstate ? "Свернуть" : "Развернуть";

        if ($(this).attr('state') === '0') {
            target.find('.collapse:not(.in)').each(function () {
                $(this).collapse('show');
            });
        }
        else {
            target.find('.collapse.in').each(function () {
                $(this).collapse('hide');
            });
        }

        $(this).html("<i class=\"fa fa-" + icon + "\"></i> " + text + " все");

        $(this).attr('state', newstate);
    });

    $(document).on('click', '.can-submit', function (e) {
        e.preventDefault();
        $(this).closest('form').submit();
    });

    $(document).on('click', '.cafe-nav li > a', function () {
        $('.cafe-nav li').removeClass('active');
        $(this).parent().addClass('active');
    });
    $(document).on('click', '.scroll-top', function () {
        $("html, body").animate({scrollTop: 0}, "slow");
    });

    $(document).on('change', 'input[type=text]', function () {
        var trimmed = $(this).val().trim();
        $(this).val(trimmed);
    });

    $(window).scroll(function () {
        if ($(this).scrollTop() > 100) {
            $('.scroll-top').fadeIn();
        } else {
            $('.scroll-top').fadeOut();
        }
        var isBottom = $(window).scrollTop() + $(window).height() === $(document).height();
        $('.scroll-top').toggleClass('bottom', isBottom);
    });

    $('.cart-order-actions input:submit').click(function (e) {
        e.preventDefault();
        var address = $('#Address_DeliveryAddress');
        var indOrder = $('#DeliveryAddress').val();
        var compOrder = $('#delivery-address-btn span').data("full-address");
        var form = $('#delivery-form')
        if (compOrder !== undefined && compOrder !== '') {
            address.val(compOrder);
        } else if (indOrder !== undefined && indOrder !== '') {
            address.val(indOrder);
        }
        form.submit();
    });

    initCategoryBar();

    addChangePasswordRules();
}

//смена направления стрелочек при открытии/закрытии категориий в разделе "Меню" и "Блюда"
function changeDirectionCollapsed(el) {
    if ($(el).hasClass('collapsed'))
        $(el).removeClass('collapsed');
    else
        $(el).addClass('collapsed');
}

function initDatePickers() {
    // Инициализация календаря на страницах
    var pickerConfig = {
        //dateFormat: "dd.mm.yy",
        //firstDay: 1,
        //showButtonPanel: true,
        //onClose: function () {
        //    val = $(this).datepicker('getDate');
        //    $(this).datepicker("setDate", val);
        //}
    };

    $('.input-daterange input, .input-group.date>input, .input-date input')
        .on('click', e => e.preventDefault())
        .datepicker(pickerConfig);

    $('.input-group.date span').on('click', function () {
        $(this).prev().datepicker('show');
    });

    $.datepicker.regional['ru'] = {
        closeText: "Закрыть",
        prevText: "&#x3C;Пред",
        nextText: "След&#x3E;",
        currentText: "Сегодня",
        monthNames: ["Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
            "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"],
        monthNamesShort: ["Янв", "Фев", "Мар", "Апр", "Май", "Июн",
            "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек"],
        dayNames: ["воскресенье", "понедельник", "вторник", "среда", "четверг", "пятница", "суббота"],
        dayNamesShort: ["вск", "пнд", "втр", "срд", "чтв", "птн", "сбт"],
        dayNamesMin: ["Вс", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб"],
        weekHeader: "Нед",
        dateFormat: "yy-mm-dd",
        firstDay: 1,
        isRTL: false,
        showMonthAfterYear: false,
        yearSuffix: "",
        showButtonPanel: true,
        onClose: function () {
            val = $(this).datepicker('getDate');
            $(this).datepicker("setDate", val);
        }
    };
	
    $.datepicker.setDefaults($.datepicker.regional['ru']);
	
    $('.input-daterange input, .input-group.date>input, .input-date input')
        .on('click', e => e.preventDefault())
        .datepicker(pickerConfig);

    $('.input-group.date span').on('click', function () {
        $(this).prev().datepicker('show');
    });

    var timepickerConfig = {
        closeText: "Закрыть",
        prevText: "&#x3C;Пред",
        nextText: "След&#x3E;",
        currentText: "Сегодня",
        monthNames: ["Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
            "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"],
        monthNamesShort: ["Янв", "Фев", "Мар", "Апр", "Май", "Июн",
            "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек"],
        dayNames: ["воскресенье", "понедельник", "вторник", "среда", "четверг", "пятница", "суббота"],
        dayNamesShort: ["вск", "пнд", "втр", "срд", "чтв", "птн", "сбт"],
        dayNamesMin: ["Вс", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб"],
        weekHeader: "Нед",
        dateFormat: "yy-mm-dd",
        timeText: 'Время',
        hourText: 'Часы',
        minuteText: 'Минуты',
        separator: 'T',
        firstDay: 1,
        isRTL: false,
        showMonthAfterYear: false,
        yearSuffix: "",
        showButtonPanel: true,
        onClose: function () {
            const val = $(this).datetimepicker('getDate');
            $(this).datetimepicker("setDate", val);
        },
    };
    $('.input-group.datetime>input').datetimepicker(timepickerConfig);

    $('.input-group.datetime span').on('click', function () {
        $(this).prev().datetimepicker('show');
    });
}

// Функция для подключения фильтра по категориям
function initCategoryBar() {
    $('#category-anchors a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
        // Находим таб, который был активирован, и получаем его атрибут href.
        // В этом атрибуте находится id дива нужной категории. В div с таким id нужно будет загружать блюда:
        var anchor = $(this).attr("href");
        // Если выбрано отображение всех категорий - тогда загружать ничего не надо, все категории уже были загружены при отображении главной страницы:
        if (anchor == "#category-all") return;

        // Сначала нужно получить идентификатор категории, которую надо показать. Идентификатор можно выделить из полученной выше переменной anchor. В anchor должна быть строка вида "#category-49". Чтобы получить id категории, надо вырезать из строки "#category-" и оставшееся перевести в число.
        const ancLenhgth = "#category-".length;
        const catId = anchor.substr(ancLenhgth, anchor.length - ancLenhgth);
        // Теперь надо проверить, загружены уже блюда категории или ещё нет.
        // Если блюда загружены - значит, должен быть элемент с id вида "RestOfCatFilt49":
        const divFilt = document.getElementById("RestOfCatFilt" + catId);
        if (!divFilt) {
            // Получение параметра URL кафе - на случай, если фильтр работает на странице кафе:
            const cafeUrl = $("#hidCafeUrlParam").val();
            // Если фильтр работает на странице кафе - тогда надо просто скопировать данные из div всех категорий в div фильтра нужной категории.
            if (cafeUrl.length > 0) {
                const data = $("div[data-category='" + anchor + "'] .category-content").html();
                if (data) {
                    $(anchor + ' .category-content').html(data);
                }
            }
            else { // Если же фильтр работает на главной странице - тогда надо выполнять ajax-запрос к серверу и получать блюда из нужной категории.
                // Загрузка блюд категории с сервера:
                $.get("/Cafe/TopDishesFromCat/" + catId, function (catDishes) {
                    //$(anchor + ' .category-content').html(catDishes);
                    $(anchor).html(catDishes);
                });
                //$(anchor + ' .category-content').html("Пожалуйста, подождите...");
                $(anchor).html("Пожалуйста, подождите...");
            }
        }
        // Убираем все дивы категорий с дополнительными блюдами с таба "все категории":
        $('.rest-content').css('display', 'none');
        // Показываем кнопки отображения уже загруженных блюд на табе "все категории":
        $('.btn-rest-loaded').css('display', '');
    });
}

// Эта функция должна показывать дивы с дополнительными блюдами (кроме первых 6) и убирать кнопки загрузки/отображения уже загруженных блюд. В случае загрузки также надо назначить дополнительный класс дополнительным кнопкам отображения загруженных блюд, чтобы они скрывались/показывались скриптом фильтра
function showRestHideBtns(catId, isAlreadyLoaded) {
    // Показываем дивы с остальными блюдами для категории:
    var restDataCat = 'catrest-' + catId;
    $('div[data-category="' + restDataCat + '"]').css('display', '');
    // Атрибут категории для кнопок отображения уже загруженных блюд:
    var btn2DataCat = 'catbtn2-' + catId;
    if (isAlreadyLoaded) {
        // Сработала кнопка отображения уже загруженных блюд - надо только убрать саму кнопку:
        $('input[data-category="' + btn2DataCat + '"]').css('display', 'none');
    }
    else {
        // Сработала кнопка загрузки блюд
        // Убираем кнопки загрузки:
        var btn1DataCat = 'catbtn1-' + catId;
        $('input[data-category="' + btn1DataCat + '"]').css('display', 'none');
        // Назначаем дополнительный класс кнопкам отображения уже загруженных блюд:
        $('input[data-category="' + btn2DataCat + '"]').addClass('btn btn-more btn-rest-loaded');
    }
}

// Эта функция должна показывать дивы с дополнительными блюдами (кроме первых 6) и убирать кнопки загрузки/отображения уже загруженных блюд. В случае загрузки также надо назначить дополнительный класс дополнительным кнопкам отображения загруженных блюд, чтобы они скрывались/показывались скриптом фильтра
function showRestHideBtns(catId, isAlreadyLoaded) {
    // Показываем дивы с остальными блюдами для категории:
    var restDataCat = 'catrest-' + catId;
    $('div[data-category="' + restDataCat + '"]').css('display', '');
    // Атрибут категории для кнопок отображения уже загруженных блюд:
    var btn2DataCat = 'catbtn2-' + catId;
    if (isAlreadyLoaded) {
        // Сработала кнопка отображения уже загруженных блюд - надо только убрать саму кнопку:
        $('input[data-category="' + btn2DataCat + '"]').css('display', 'none');
    }
    else {
        // Сработала кнопка загрузки блюд
        // Убираем кнопки загрузки:
        var btn1DataCat = 'catbtn1-' + catId;
        $('input[data-category="' + btn1DataCat + '"]').css('display', 'none');
        // Назначаем дополнительный класс кнопкам отображения уже загруженных блюд:
        $('input[data-category="' + btn2DataCat + '"]').addClass('btn btn-more btn-rest-loaded');
    }
}

/*function configureNotify() {
 $.notify.defaults({
 style: 'metro',
 className: 'info',
 globalPosition: 'bottom right',
 autoHideDelay: 10000
 });
 };*/

function updateSideBar() {
    var cartType = $('#shortCart').data('carttype');
    if ($('#sidebar-cart-totals')) {
        $.ajax({
            type: "GET",
            url: "/Cart/GetTotals?cartType=" + cartType,
            traditional: true,
            success: function (data) {
                $('#sidebar-cart-totals').replaceWith(data);
            }
        });
    }

    if ($('.cart-item').length === 0) {
        if (document.getElementById('id-delivery-info') !== null) {
            $.ajax({
                type: "POST",
                url: "/Cart/ResetOrder?cartType=Full",
                traditional: true,
                dataType: "html",
                success: function (html) {
                    $('#list').replaceWith("<div id=\"list\" class=\"main cart row peculiar\">" + html + "</div>");
                    $("#id-delivery-info").remove();
                }
            });
        } else {
            $.ajax({
                type: "POST",
                url: "/Cart/ResetOrder?cartType=" + cartType,
                traditional: true,
                dataType: "html",
                success: function (html) {
                    $('#shortCart').html(html);
                }
            });
        }
    } else {
        var cards = $('.sidebar-cart-items .card');
        for (var i = 0; i < cards.length; i++) {
            var card = cards[i];
            if ($(card).find('.cart-item').length === 0) {
                $(card).remove();
            }
        }
    }
}

function orderExists(xhr, status, error) {
    if (xhr.status === 403) {
        $('#orderExistsModal').modal();
    }
}

function scrollToTop() {
    $("html, body").animate({scrollTop: 0}, "slow");
}

function agreeWithTerms() {
    $('#confirmAgreement').change(function () {
        var submitBtn = $('#shortCart').find('input[type=submit]');

        submitBtn.attr('disabled', !this.checked);
    });
}

function orderExists(cleanurl, name) {
    var modal = $('#orderExistsModal');
    var target = $(modal).find('.order-exist-cafe');
    target.attr('href', cleanurl);
    target.text(name);

    modal.modal();
}

//Обновляет значение баллов после заказа
function updateTotalPoints() {
    if ($('#id-total-points')) {
        $.ajax({
            type: "GET",
            url: "/Cafe/GetTotalPoints",
            traditional: true,
            success: function (data) {
                $('#id-total-points').html(data);
            }
        });
    }
    ;
}

//Проверяем хватает ли баллов для оплаты
function checkPaidPoints(checkBox, totalPoints) {
    $.ajax({
        type: "GET",
        url: "/Cart/GetTotalPriceAndDeliveryCost",
        traditional: true,
        success: function (data) {
            if (data > totalPoints) {
                checkBox.checked = false;
                $('#paid-points-message').html("Недостаточно баллов");

            }
            ;
        }
    });
}

// Обрезает длинное имя пользователя
var wrapSize = 25,
    wrapContent = $('.nav-user-name'),
    wrapText = wrapContent.text();

if (wrapText.length > wrapSize) {
    wrapContent.text(wrapText.slice(0, wrapSize) + '...');
}


function updateCategoriesBar() {
    $.ajax({
        type: "POST",
        url: "/Cafe/UpdateCategoriesByFilter",
        traditional: true,
        success: function (data) {
            $("#category-anchors").html(data);
            initCategoryBar();
        }
    })
}

var dishDetailTimeout;

function showDishDetails(dish) {
    if ($(window).width() >= 1200) {
        var popupDetail = $(dish).find('.dish-detail');
        dishDetailTimeout = setTimeout(function () {
            popupDetail.show();
        }, 1000);
    }
}

function hideDishDetails(dish) {
    var popupDetail = $(dish).find('.dish-detail');
    popupDetail.hide();
    clearTimeout(dishDetailTimeout);
}

function order() {
    $(document).on("click", "#id-delivery-info .dropdown li a", function () {
        var text = $(this).text();
        var fullAddress = $(this).data("full-address");
        var cartType = $('#shortCart').data('carttype');

        $('#id-delivery-info .dropdown span')
            .text(text)
            .attr("data-full-address", fullAddress);
        $('#Address_DeliveryAddress').val(fullAddress);
        $.ajax({
            url: "/Cart/SelectNewShippingAddress",
            data: {
                "address": fullAddress,
                "cartType": cartType,
            },
            type: "POST",
            async: false,
            success: (html) => {
                $('#shortCart').html(html);
            }
        });
    });

    $(document).on("click", "#Address_IsCompanyOrder", function () {
        var type = $(this).is(':checked') ? true : false;
        var companyId = $(this).data('companyid');
        $.ajax({
            type: "POST",
            url: "/Cart/UpdateCompanyAddress",
            data: {
                "isCompanyOrder": type
            },
            traditional: true,
            success: function (data) {
                $('#company-delivery-address').replaceWith(data);
                var form = $('#delivery-form')
                    .removeData("validator")
                    .removeData("unobtrusiveValidation");

                $.validator.unobtrusive.parse(form);
                $('#Address_DeliveryAddress').val('');
            }
        });
    });
}

function CloseNewOrdersMessage(url) {
    $("#new-orders-message").hide();
    $.ajax({
        type: "POST",
        url: url,
        traditional: true
    });
}

function RaiseOrder(caffeeId) {
    $("#new-orders-message").text($("#new-orders-message").text() + caffeeId);
    $("#new-orders-message").show();
    $("#new-orders-message").addClass("order-in-caffee-" + caffeeId);
}

function CheckNewOrders() {
    $("div[class^='order-in-caffee-']").show();
}

function HideNewOrdersMessage() {
    $("#new-orders-message").removeClass(function (index, css) {
        return (css.match(/\bborder-in-caffee-\S+/g) || []).join(' '); // removes anything that starts with "order-in-caffee-"
    });
}

//Показывавает оповещение
function ShowMessageModal() {
    $('#messageModal').modal('show');
}


//Установить маску для телефона
function setPhoneMask(id, unswallowWorkaround) {
    if (unswallowWorkaround) {
        var initial = $(id).val();
        if (typeof initial === 'string' && initial[0] !== '+') {
            $(id).val('+' + initial);
        }
    }
    $(id).inputmask({ mask: "+7 (999) 999 9999" });
}



//beg Работа с таймером на странице Входа на сайт

function runLoginSmsCooldown() {
    runCountdown(120, {
        onStart: function() {
            $("#id-js-link-resend").hide();
            $("#id-timer").show();
            $('#SmsCode').removeAttr('readonly');
        },
        onTick: function(remain) {
            $("#id-js-timer").html(remain);
        },
        onEnd: function () {
            $("#id-timer").hide();
            $("#id-js-link-resend").show();

            // Делаем поле "Код из СМС" не активным (срок действия кода истек).
            $('#SmsCode').attr('readonly', 'readonly');
        }
    });
}

//Нажатие на кнопку "Отправить еще раз"
function resendSmsCode(url) {
    url += "?phone=" + encodeURIComponent($('#Phone').val());

    $('#id-js-link-resend').hide();
    $('#id-js-link-resend-spinner').show();
    
    $.ajax({
        method: "GET",
        url: url,
        success: function (data) {
            $('#id-ajax-message').html(data);
            runLoginSmsCooldown();
        },
        error: function (xhr, status, err) {
            $('#id-js-link-resend').show();
            $('#id-ajax-message').html(status === "error" ? xhr.responseText : err);
        },
        complete: function () {
            $('#id-js-link-resend-spinner').hide();
        }
    });
}

//end Работа с таймером на странице Входа на сайт

// Функция обновления стоимости доставки в корзине
// при заказе на разные даты
function updateDeliveryCost(cafeId, dateStr, url) {
    var deliveryDiv = $('#delivery-' + cafeId + '-date-' + dateStr);
    if (deliveryDiv.length > 0) {
        $.ajax({
            method: "GET",
            url: url,
            success: function (data) {
                deliveryDiv.html(data);
            }
        });
    }
}

function ChangeAddress(Id, id) {
    $.ajax({
        url: "/Orders/ChangeAddress",
        data: {
            "OrderId": Id,
            "AddressId": id,
        },
        type: "POST",
        async: false,
        success: function () {
            window.location.reload();
        }
    });
}

function clearImage(elementId) {
    document.getElementById(elementId + 'Preview').innerHTML = '';
    $('#' + elementId).val('');
}

function handleFileSelect(evt, validationSpan, elementId, maxSize, maxWidth, maxHeight, minWidth, minHeight, displayWidth = 0, displayHeight = 0) {
    var spanMessage = document.getElementById(validationSpan);
    spanMessage.innerText = '';
    // Получение наличия выбранного файла
    var imgFile = evt.target.files[0];
    if (!imgFile) return;
    // Проверка типа и размера файла
    if (!imgFile.type.match('image.*')) {
        spanMessage.innerText = 'Файл не является изображением';
        return;
    }
    if (imgFile.size > maxSize) {
        spanMessage.innerText = 'Файл слишком большой, допустимый размер не более ' + maxSize / 1024 + ' Кб';
        return;
    }
    // Загрузка данных файла
    var reader = new FileReader();
    // Установка обработчиков успешной загрузки и ошибки
    reader.onload = function () {
        var imgNatural = document.createElement('img');
        imgNatural.src = reader.result;
        imgNatural.onload = function () {
            var span = document.getElementById(validationSpan);
            var imgWidth = imgNatural.width;
            var imgHeigh = imgNatural.height;
            if (imgWidth > maxWidth || imgHeigh > maxHeight) {
                span.innerText = 'Изображение слишком большое, допустимый размер не более ' + maxWidth + 'x' + maxHeight;
                return;
            }
            else if (imgWidth < minWidth || imgHeigh < minHeight) {
                span.innerText = 'Изображение слишком маленькое, минимальный размер ' + minWidth + 'x' + minHeight;
                return;
            }
            var imgHtml = '';
            if (displayWidth > 0 && displayHeight > 0) {
                imgHtml = "<img id='Loaded${elementId}FilePreview' src='" + reader.result + "' width='" + displayWidth + "' height='" + displayHeight + "' />";
            }
            else {
                imgHtml = "<img id='LoadedImageFilePreview' src='" + reader.result + "' class='img-responsive center-block' />";
            }
            document.getElementById(elementId + 'Preview').innerHTML = imgHtml;
        };
    };
    reader.onerror = function (error) {
        document.getElementById(validationSpan).innerText = 'Ошибка загрузки изображения: ' + error;
    };
    reader.readAsDataURL(imgFile);
}

function addChangePasswordRules() {
    $('#NewPassword').rules('add', { notEqual: 'OldPassword', messages: { notEqual: "Старый и новый пароль не должны совпадать" } });
}

function hiddenDropDownList(item, id) {
    if (item.text != "Как можно скорее") {
        document.getElementById(id).style.display = "none";
        var string = $(item).children('[value="' + item.value + '"]').text();
        string = 'с ' + string.replace(' - ', ' до ');
        changeDeliveryTime(id);
        document.getElementById('deliveryTimeLabel_' + 0).style.display = "block";
        document.getElementById('deliveryTimeLabel_' + id).textContent = string;
        document.getElementById('Clear_' + id).style.display = "";
    } 
}
function hiddenDropDownListByChild(item, id) {
    if (item.text != "Как можно скорее") {
        document.getElementById(id).style.display = "none";
        var string = item.text;
        string = 'с ' + string.replace(' - ', ' до ');
        document.getElementById('deliveryTimeLabel_' + id).textContent = string;
        document.getElementById('Clear_' + id).style.display = "";
    }
}
function showDropDownList(id) {
    var dropDown = document.getElementById(id);
    if (dropDown.style.display == "none") {
        dropDown.style.display = "";
        dropDown.selectedIndex = 0;
        document.getElementById('deliveryTimeLabel_' + id).textContent = "";
        document.getElementById('Clear_' + id).style.display = "none";
        changeDeliveryTime(id);
        document.getElementById('deliveryTimeLabel_' + 0).style.display = "none";
    }
}

function changeDeliveryTime(id) {
    $.ajax({
        url: "/Cart/UpdateCart",
        data: {
            cafeId: $('[name="date_' + id + '"]').data("cafe-id"),
            index: id,
            date: $('[name="date_' + id + '"]').val(),
            time: $('[name="time' + id + '"]').val()
        }
    });
}


function hiddenMoneyComment(idSelect) {
    if ($(idSelect).val() == 1 && $('#noChange').prop('checked') == false)
        displayValue = "block";
    else if ($(idSelect).val() != 1 && $('#noChange').prop('checked') == false)
        displayValue = "none";
    else if ($(idSelect).val() != 1 && $('#noChange').prop('checked') == true)
        displayValue = "none";
    else if ($(idSelect).val() == 1 && $('#noChange').prop('checked') == true) {
        displayValue = "block";
        $(".hidden-money-comment").each(function (index, el) {
            $(el).css("display", displayValue);
        });
        hiddenMoneyCommentLabel($('#noChange').prop('checked'));
        return;
    }
    $(".hidden-money-comment").each(function(index, el) {
        $(el).css("display", displayValue);
    });
}


function hiddenMoneyCommentLabel(idSelect) {
    if ($(idSelect).prop('checked') == true) displayValue = "none";
    else if (idSelect == true) displayValue = "none";
    else
        displayValue = "block";
    $(".hidden-money-comment-label").each(function (index, el) {
        $(el).css("display", displayValue);
    });
}

/**
 * Запускает отсчет вниз.
 *
 * @param {number} durationSec время, сек.
 * @param {object} options коллбэки
 */
function runCountdown(durationSec, options) {
    if (!Number.isInteger(durationSec) || durationSec < 0)
        throw new Error("Bad duration: " + durationSec);

    var noop = function () { };
    var onStart = options && typeof options.onStart == 'function' ? options.onStart : noop;
    var onTick = options && typeof options.onTick == 'function' ? options.onTick : noop;
    var onEnd = options && typeof options.onEnd == 'function' ? options.onEnd : noop;
    var remain = durationSec;

    onStart();
    onTick(durationSec);

    if (durationSec === 0) {
        onEnd();
        return;
    }

    var interval = setInterval(function () {
        remain -= 1;

        onTick(remain);

        if (remain <= 0) {
            onEnd();
            clearInterval(interval);
        }
    }, 1000);

}