$(document).ready(function () {
    managerMenu();
})

function eventHandlers() {
    $('#menu-patterns #PatternName').on('input', function () {
        var val = $(this).val();
        if (val !== '') {
            $('#add-pattern-btn').removeAttr('disabled');
            $('#IsBanket').removeAttr('disabled');
        } else {
            $('#add-pattern-btn').attr('disabled', true);
            $('#IsBanket').attr('disabled', true);
        }
    })

    $('#menu-patterns select').on('change', function () {
        var url = $(this).data('url');
        var target = $(this).data('target');
        var id = $(this).val();
        $.ajax({
            type: 'POST',
            url: url,
            data: {
                id: id
            },
            success: function (data) {
                $(target).html(data);
                if (id !== '') {
                    $('#update-menu-form').find('button').removeAttr('disabled');
                    $('#manager-menu').attr('data-preview', true);
                    $('#del-pattern-btn').removeAttr('disabled');
                    $('#add-pattern-btn').removeAttr('disabled');
                    $('#cancel-pattern-btn').removeAttr('disabled');
                    $('#IsBanket').removeAttr('disabled');
                } else {
                    $('#manager-menu').attr('data-preview', false);
                    $('#update-menu-form').find('button').attr('disabled', 'disabled');
                    $('#del-pattern-btn').attr('disabled', true);
                    $('#add-pattern-btn').attr('disabled', true);
                    $('#cancel-pattern-btn').attr('disabled', true);
                    $('#IsBanket').attr('disabled', true);
                }
                $('#save-menu-form').find('#PatternName').val($('#select-pattern-form').find('option:selected').text());
                eventHandlers();
            }
        });
    });
}

function getDocumentHandler() {

    $(document).ready(function () {
        $('.selectable').selectable({
            filter: ".selectable-item",
            cancel: "span,.cancel-select,a"
        });

        $('.document-type').on('click', function (e) {
            var btn = $(this);
            e.preventDefault();
            var data = btn.closest('form').serializeArray();
            $('.selectable .ui-selected').each(function () {
                data.push({
                    name: $(this).data('name'),
                    value: $(this).data('item-id')
                });
            });
            data.push({ name: "X-Requested-With", value: "XMLHttpRequest" });
            $.ajax({
                type: "POST",
                url: btn.attr('href'),
                data: data,
                success: function (data, status, xhr) {
                    var target = btn.attr('target');
                    if (target) {
                        var newWin = window.open(data, target);
                        $(newWin).ready(function () {
                            setTimeout(function () {
                                newWin.print();
                            }, 1000);
                        });
                    } else {
                        window.location = data;
                    }
                }
            });

        });

        $('[name=SortType]').change(function () {
            $('#form0').submit();
        });
    }).keypress(function (e) {
        if (e.keyCode === 27) {
            $('.selectable .ui-selected').removeClass('ui-selected');
        }
    });

    $('.expand-order').on('click', function (e) {
        var elem = $(this);
        var load = elem.attr('data-load');
        if (load) {
            var id = elem.parent().parent().data('item-id');
            var sortType = $('[name=SortType][checked]').val();
            var url = '/manager/reports/userorders?companyorderId=' + id + '&SortType=' + sortType;
            var target = elem.data('target');
            if (!id) {
                id = elem.parent().parent().data('banket-id');
                url = '/manager/reports/banketorders?banketId=' + id;
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

function expandOrderItems(target) {
    $(target).on('click', '.expand-order-items', function () {
        var elem = $(this);
        var load = elem.attr('data-load');
        if (load) {
            var id = elem.parent().parent().data('item-id');
            $.ajax({
                url: '/manager/reports/orderitems?orderid=' + id,
                success: function (data) {
                    $('#order-positions-' + id).html(data);
                    elem.removeAttr('data-load')
                }
            })
        }
    })
}

function saveMenu(update) {
    var form = $('#save-menu-form');
    var dishes = $('#schedule-dishes').find('form').serialize();
    if (form.valid() && dishes.length > 0) {
        ;
        var url = $(form).attr('action');
        var name = $(form).serialize();
        var a = false;
        if (update) {
            a = true;
        }
        $.ajax({
            type: 'POST',
            url: url + '?' + name + '&approve=' + a,
            async: false,
            processData: false,
            traditional: true,
            data: dishes,
            success: function (data) {
                $($(form).data('ajax-update')).replaceWith(data);
                eventHandlers();
                if (update) {
                    $('#approveModal').modal('hide');
                }
            },
            error: function () {
                $('#approveModal').modal('show');
            }
        })
    }
}

function updateMenuByPtternId(el) {
    var patternId = $('#menu-patterns #PatternId').val();
    if (patternId !== '') {
        var form = $(el).closest('form');
        form.find('#PatternId', patternId);
        form.submit();
    }
}

function move(srcSelector, destSelector, element, actionType) {
    var categoryId = element.data('category');

    var panelSelector = '.panel[data-category=' + categoryId + ']';
    var src = $(srcSelector);
    var srcPanel = src.children(panelSelector);

    var dest = $(destSelector);
    var destPanel = dest.children(panelSelector);
    var destUl = null;
    if (!destPanel[0]) {
        dest.append(srcPanel.clone(true, true));
        destPanel = dest.children(panelSelector);

        var anchor = destPanel.find('a[data-toggle=collapse]')
        var collapseTarget = anchor.attr('data-target');
        collapseTarget = collapseTarget.replace(srcSelector, destSelector);
        anchor.attr('data-target', collapseTarget);


        destUl = destPanel.find('> ul');
        destUl.empty();
    }

    destUl = destPanel.find('ul');
    destUl.append(element);
    element.data('category', categoryId);
    if (actionType === 0) {
        $(element).find('.add-action').replaceWith($('#remove-action').html())
    } else {
        $(element).find('.remove-action').replaceWith($('#add-action').html())
    }

    if (srcPanel.find('ul').children().length === 0)
        srcPanel.remove();
}

function removeDeliveryCostItem(id) {
    $('#Delivery-cost-' + id).remove();
};

function editDeliveryCostItem(index) {
    $('#Delivery-cost-' + index + " input").prop('readonly', false);
    $('#Delivery-cost-' + index + " input[name*=IsEdit]").attr('value', true);
}

function removeDish(id) {
    var removeCatId = $("#remove-cat-id").val();
    $("#dish-" + id).detach().appendTo(removeCatId);
}

//Обновляет индексы в категории, из которой удалили блюдо
function updateIndexInCategory(categoryId) {
    $('[data-category = ' + categoryId + ']').children('ul').children('li').each(function () {
        $(this).data('dish-index', $(this).index() - 1);
    });
}

function managerMenu() {
    $('#manager-dishes .sortable').sortable({
        connectWith: ".sortable",
        containment: ".main",
        items: ".sortable-item",
        //cancel: "input,textarea,button,select,option,a",
        cursor: "move",
        sort: function (event, ui) {
            ui.helper.css({ 'top': ui.position.top + $('html').scrollTop() + 'px' });
        },
        update: function (e, ui) {

            if (ui.sender) return;
            var newCategoryId = 0;
            var newCategoryName = '';

            var url = $('#active-categories').data('change-dish-url');
            if ($(this).data('category') !== ui.item.closest('div').data('category')) {
                url = $('#active-categories').data('update-dish-index-url');
                newCategoryId = ui.item.closest('div').data('category');
                newCategoryName = ui.item.closest('div').data('category-name');
            }

            if (newCategoryName.toLowerCase() == 'блюда удаленных категорий') {
                $(this).sortable('cancel');
                return;
            }

            var $element = $(this);

            //проверяем, чтобы блюдо прикрепилось к ul и не div
            let liDish = ui.item.first();
            let ulDishCategrory = liDish.siblings('ul').first();
            if (ulDishCategrory.length > 0) {
                //если есть сосед ul, то перемещаем блюдо в ul
                ulDishCategrory.first().append(liDish);
            }

            var oldCategoryId = $element.data('category');
            var currentIndex = ui.item.index();
            var oldIndex = ui.item.data('dish-index');
            if (oldIndex === '') oldIndex = -1;

            var dishId = ui.item.attr('id').substr(5);

            $.ajax({
                type: "POST",
                url: url,
                traditional: true,
                async: false,
                contentType: "application/x-www-form-urlencoded; charset=UTF-8",
                data: {
                    'newCategoryId': newCategoryId,
                    'oldCategoryId': oldCategoryId,
                    'newIndex': currentIndex,
                    'oldIndex': oldIndex,
                    'dishId': dishId
                },
                success: function (res) {
                    $element.children('ul').children('li').each(function () {
                        $(this).data('dish-index', $(this).index());
                    });
                    if (newCategoryId == 0) {
                        updateCategoriesPanels(res);
                    }
                },
                error: function () {
                    $(this).sortable('cancel');
                }
            });
        },
        receive: function (e, ui) {

            var $element = $(this);
            var categoryId = $(this).data('category');
            if ($(this).data('category-name').toLowerCase() == 'блюда удаленных категорий') {
                $(this).sortable('cancel');
                return;
            }
            var currentIndex = ui.item.index();//-1
            var url = $('#active-categories').data('add-dish-url');
            var oldIndex = ui.item.data('dish-index');
            var dishId = ui.item.attr('id').substr(5);

            $.ajax({
                type: "POST",
                url: url,
                traditional: true,
                async: false,
                contentType: "application/x-www-form-urlencoded; charset=UTF-8",
                data: {
                    'categoryId': categoryId,
                    'newIndex': currentIndex,
                    'dishId': dishId
                },
                success: function (res) {
                    $element.children('ul').children('li').each(function () {
                        $(this).data('dish-index', $(this).index()); //-1
                    });
                    updateCategoriesPanels(res);
                },
                error: function () {
                    $(this).sortable('cancel');
                }
            });
        }
    });

    $('#manager-dishes .sortable-panels').sortable({
        connectWith: ".sortable-panels",
        containment: ".main",
        items: ".panel",
        //cancel: "input,textarea,button,select,option,a",
        cursor: "move",
        handle: ".panel-heading",
        sort: function (event, ui) {
            ui.helper.css({ 'top': ui.position.top + $('html').scrollTop() + 'px' });
        },
        update: function (e, ui) {
            // Protection from unwanted firing on update event
            // only when category moves in same group
            if (ui.sender)
                return;

            var $element = $(this);
            if ($element[0] !== ui.item.closest('.sortable-panels')[0]) {
                //console.log("cancel move")
                //$element.sortable('cancel');
                return;
            }
            var url = $element.data('change-url');
            if (!url)
                return;

            var currentIndex = ui.item.index();
            var oldIndex = ui.item.data('index');
            var categoryId = ui.item.data('category');
            var offset = currentIndex - oldIndex;

            $.ajax({
                type: "POST",
                url: url,
                traditional: true,
                async: false,
                contentType: "application/x-www-form-urlencoded; charset=UTF-8",
                data: {
                    'categoryId': categoryId,
                    'offset': offset
                },
                success: function (res) {
                    $('.card-content').first().children('.panel').each(function () {
                        $(this).data('index', $(this).index());
                    });
                    updateCategoriesPanels(res);
                },
                error: function () {
                    $(element).sortable('cancel');
                }
            });
        },
        receive: function (e, ui) {
            var $element = $(this);
            var url = $element.data('add-url') || $element.data('remove-url');
            if (!url)
                return;

            var currentIndex = ui.item.prev().data('source-index');
            var offset = currentIndex + 10 || 10;

            $.ajax({
                type: "POST",
                url: url,
                traditional: true,
                async: false,
                contentType: "application/x-www-form-urlencoded; charset=UTF-8",
                data: {
                    'categoryId': ui.item.data('category'),
                    'offset': offset
                },
                success: function (res) {
                    $('#active-categories').find('.panel').each(function () {
                        $(this).data('index', $(this).index());
                        $(this).data('source-index', 10 + $(this).index() * 10);
                    });

                    $('#inactive-categories').find('.panel').each(function () {
                        $(this).data('index', -1);
                    });
                    updateCategoriesPanels(res);
                },
                error: function () {
                    $(element).sortable('cancel');
                }
            });
        }
    });
}

function managerHistory() {
    $(document).ready(function () {
        $(document).on('click', '.category-anchors-manager a', function (e) {
            e.preventDefault();
            var anchor = $(this).attr('href');
            var target = $(anchor).find(".collapse");
            var expanded = target.attr('aria-expanded');
            if (expanded === 'false')
                $(target).collapse('show');
            $('html, body').animate({
                scrollTop: target.offset().top - target.prev().outerHeight(true) - 100,
            }, 500);
        });
    })
}

function prepareFormData(form, action, preview, date) {
    if (form.find('#ActionType').length > 0) {
        form.find('#ActionType').val(action);
    } else {
        var actionType = document.createElement('input');
        actionType.id = 'ActionType';
        actionType.type = 'hidden';
        actionType.value = action;
        actionType.name = 'ActionType';
        form.append(actionType.outerHTML);
    }
    if (form.find('#ToDate').length > 0) {
        form.find('#ToDate').val(date);
    } else {
        var toDate = document.createElement('input');
        toDate.id = 'ToDate';
        toDate.type = 'hidden';
        toDate.value = date;
        toDate.name = 'ToDate';
        form.append(toDate.outerHTML);
    }

    if (form.find('#IsPreview').length > 0) {
        form.find('#IsPreview').val(preview);
    } else {
        var isPreview = document.createElement('input');
        isPreview.id = 'IsPreview';
        isPreview.type = 'hidden';
        isPreview.value = preview;
        isPreview.name = 'IsPreview';
        form.append(isPreview.outerHTML);
    }
    var categoryId = form.closest('li').data('category');
    var category = document.createElement('input');
    category.id = 'CategoryId';
    category.type = 'hidden';
    category.value = categoryId;
    category.name = 'CategoryId';
    form.append(category.outerHTML);
    return form;
}

function addListeners(id) {
    var el = document;
    if (id) {
        el = id;
    }

    $(el).on('submit', '.menu-dish', function (e) {
        e.preventDefault();
        return false;
    });

    $(el).on('click', '.add-action', function () {
        addToMenu(this);
    });

    $(el).on('click', '.remove-action', function () {
        removeFromMenu(this);
    });

    $(el).on('input', '.price input', function () {
        changePrice(this);
    });
}

function addToMenu(el) {
    var preview = $('#manager-menu').data('preview');

    var form = prepareFormData($(el).closest('form'), 0, preview, $('#datepicker input').val());
    if (!preview) {
        handleMenuForm(form)
    }
    move('#available-dishes', '#schedule-dishes', form.parent(), 0);
}

function removeFromMenu(el) {
    var preview = $('#manager-menu').data('preview');
    var form = prepareFormData($(el).closest('form'), 1, preview, $('#datepicker input').val());

    if (!preview) {
        handleMenuForm(form);
    }
    move('#schedule-dishes', '#available-dishes', form.parent(), 1);
}

function changePrice(el) {
    var preview = $('#manager-menu').data('preview');
    var button = $(el).parent().children('span');
    if (button.html() != '<i class="fa fa-floppy-o"></i>') {
        button.html('<i class="fa fa-floppy-o"/>');
        button.on('click', function (e) {
            var form = prepareFormData($(this).closest('form'), 2, preview, $('#datepicker input').val());

            handleMenuForm(form);
        });
    }
}

function handleMenuForm(form) {
    var url = $(form).attr('action');
    var processIcon = $('#processIcon');
    $.ajax({
        url: url,
        type: 'POST',
        data: $(form).serialize(),
        beforeSend: function () {
            processIcon.show()
        },
        success: function (data) {
            $(form.parent()).replaceWith(data)
            addListeners($.parseHTML(data)[1])
        },
        complete: function () {
            processIcon.hide()
        }
    })
}

//смена направления стрелочек при открытии/закрытии категориий в разделе "Меню" и "Блюда"
function changeDirectionArrow(el) {
    if ($(el).hasClass('collapsed'))
        $(el).removeClass('collapsed');
    else
        $(el).addClass('collapsed');
}

//смена направления стрелочек при открытии/закрытии категориий в разделе "Меню" Развернуть/Свернуть все
function changeDirectionAllArrow(btn, idContainer, classElements) {
    if ($(btn).attr('state') == 0)
        $(idContainer + ' ' + classElements).removeClass('collapsed');
    else
        $(idContainer + ' ' + classElements).addClass('collapsed');
}


//На странице Заказы меняет статусы у всех заказов, вложенные в корпоративный.
function ajaxChangeStatusOrdersInCompany(idLink, urlAction) {

    var orders = $(idLink).closest('table').children('tbody');
    $.each(orders, function (index, value) {
        urlAction += '&orderIds=' + $(value).data('order-id');
    });

    $.ajax({
        url: urlAction,
        method: "GET",
        success: function (data) {
            orders.remove();
            $(idLink).closest('table').append(data);
        }
    });
}

function removeDishFromAllCategories() {
    $('#confirmRemove').find('.modal-body').html('<span>Если вы продолжите, то вы удалите блюдо из всех связанных категорий. Хотите продолжить?</span>');
    $('#confirmRemove').find('#delete-btn').text('Удалить');
    $('#confirmRemove').modal();
}

function saveDish() {
    var isSelected = false;
    $('#manage-category').find('[type="checkbox"]').each(function (_, el) {
        if ($(el).prop('checked')) {
            isSelected = true;
        }
    });
    if (!isSelected) {
        $('#confirmRemove').find('.modal-body').html('<span>Отсутствует связь блюда с категориями. Блюдо будет перемещено в категорию "Удаленные блюда". Хотите продолжить?</span>');
        $('#confirmRemove').find('#delete-btn').text('Подтвердить');
        $('#confirmRemove').modal();
    }
    else {
        $('#manage-category').parents('form').submit();
    }
}

function updateCategoriesPanels(content) {
    // id элемента и должен ли он быть развернут после обновления
    var map = new Map();
    $('.main').find('.collapse').each(function (i, el) {
        map.set($(el).prop('id'), $(el).hasClass('in'));
    })
    $('.main').replaceWith(content);
    for (var [key, value] of map) {
        if (value && !$('#' + key).hasClass('in')) {
            $('#' + key).addClass('in');
            changeDirectionCollapsed($('#' + key).parent().find('h4 a'));
        }
        if (!value && $('#' + key).hasClass('in')) {
            $('#' + key).removeClass('in');
            changeDirectionCollapsed($('#' + key).parent().find('h4 a'));
        }
    }
    managerMenu();
}