(function ($) {
    $(document).ready(function () {

        $(document).on('click', '.btn-clear-filter', function () {
            $('.sub-category').removeClass('active');
            $('#tags-tree input:checkbox').removeAttr('checked');
            var form = $(this).closest("form");
            setTimeout(form.submit(), 2000);
            updateCategoriesBar();
        });
        // Подствечивает родительский div тэга при выборе тега
        var timeoutId;
        $(document).on('change', '.tag-checkbox',
            function (e) {
                var form = $(this.form) || $(this).closest("form");
                if (form) {
                    // Очищаем предыдущий таймаут, чтобы запрос ожидающий оправки не ушел при поспуплении нового
                    clearTimeout(timeoutId);

                    timeoutId = setTimeout(function () {
                        form.submit();
                    }, 1000);
                }
                ;
            }
        );

        // Раскрывает/сворачивает ветку дерева тэгов по дабл клику
        $(document).on('dblclick', '.tag', function (e) {
            var target = $(this).find('a[data-toggle="collapse"]').attr("data-target");
            var expanded = $(target).attr("aria-expanded");
            if (expanded === 'true')
                $(target).collapse('hide');
            else
                $(target).collapse('show');
        });
    })
}(window.jQuery));


function selectTag(element) {
    var tag = element.parent();
    var checkBox = element.children('input[type="checkbox"]');
    var checkState = checkBox.attr('checked');

    if (checkState === undefined) {
        checkState = true;
        checkBox.attr('checked', 'checked').change();
    } else {
        checkBox.removeAttr('checked').change();
        checkState = false;
    }

    tag.toggleClass('active', checkState);
    tag.parent().children('.tree-item-children').find('.btn-tag-select').each(function () {
        selectTag($(this));
    });
};