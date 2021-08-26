(function ($) {
    $(document).ready(function () {
        $('#company-orders').DataTable({
            "language": ru,
            processing: true,
            //загрузка корпоративных заказов
            ajax: {
                type: "POST",
                url: '/curator/companyorders/getcompanyorders',
                dataSrc: ""
            },
            //можно выбрать только 1
            select: {
                style: 'single'
            },
            info: false,
            /*B - buttons,
              f - filtering input
              r - processing display element                           
              t - The table!
              p - pagination control
              l - length changing input control              
             */
            dom: 'Bfrtpl',
            responsive: true,
            length: 5,
            //сортировка по названию кафе
            order: [[1, "asc"]],
            deferRender: true,
            //кнопок никаких нет над табличкой
            buttons: [],
            "columns": [
                { "data": "id" },
                { "data": "cafe.name", "class": "wrap-word-td" },
                {
                    "data": "orderOpenDate",
                    "render": function (data) {
                        return getDateString(data);
                    }
                },
                {
                    "data": "orderAutoCloseDate",
                    "render": function (data) {
                        return getDateString(data);
                    }
                },
                {
                    "data": "deliveryDate",
                    "render": function (data) {
                        return getDateString(data);
                    }
                },
                { "data": "totalPrice" },
                {
                    "targets": -1,
                    "data": null,
                    "render": function (row) {
                        var id = row.id;
                        return '<a href = "' +
                            $('#authorLoader').data('request-url') + id +
                            '"><i class="fa fa-pencil-square-o"></i></a>';
                    }
                }
            ],
            "fnRowCallback": function (nRow, aData, iDisplayIndex) {
                // Подсвечивать отмененные корп. заказы.
                // FIXME: зашит номер статуса. Нужна нормальная модель.
                if (typeof aData == 'object' && aData.orderStatus === 5) {
                    nRow.classList.add('danger');
                } else {
                    nRow.classList.remove('danger');
                }
            },
            "aoColumnDefs": [{ "bVisible": false, "aTargets": [0] }]
        });
        $('#company-orders tbody').on('click', 'tr', function () {
            $(this).toggleClass('selected');
        });
        $('input[type=search]').css({
            "margin-left" : "10px"});
    }
    ).ajaxComplete(function () {
        
    });

}(window.jQuery));

function getDateString(date) {
    var dateObj = new Date(date);
    return dateObj.toLocaleString();
}
