(function ($) {
    $(document).ready(function () {
        $('#admin-users').DataTable({
            "language": ru,
            processing: true,
            ajax: {
                type: "POST",
                url: '/users/getusers',
                dataSrc: ""
            },
            select: {
                style: 'single'
            },
            info: false,
            dom: 'Bfrtpl',
            responsive: true,
            length: 5,
            order: [[1, "asc"]],
            deferRender: true,
            "drawCallback": function() { userActions() },
            buttons: [
                {
                    text: 'Удалить',
                    action: function (e, dt, node, config) {
                            var row = dt.row({ selected: true }).data();
                            if (row != null) {
                                var userJsonString = JSON.stringify({ user: row });
                                var userDataObject = JSON.parse(userJsonString);
                        if (confirm("Удалить пользователя?")) {
                                $.ajax({
                                    type: 'POST',
                                    url: '/users/delete',
                                    data: userDataObject.user,
                                    contentType: "application/x-www-form-urlencoded; charset=UTF-8",
                                    traditional: true,
                                    success: function (data) {
                                        dt.row({ selected: true }).remove().draw('page');
                                    }
                                });
                            }
                        }
                    }
                },
                {
                    text: 'Редактировать',
                    action: function (e, dt, node, config) {
                        var row = dt.row({ selected: true }).data();
                        if (row != null) {
                            var userJsonString = JSON.stringify({ user: row });
                            var userDataObject = JSON.parse(userJsonString);
                            $.ajax({
                                type: 'POST',
                                url: '/users/edit',
                                data: userDataObject.user,
                                contentType: "application/x-www-form-urlencoded; charset=UTF-8",
                                traditional: true,
                                async: false,
                                success: function (data) {
                                    var target = $('#userDetailsModal').replaceWith(data);
                                    setTimeout("$('#userDetailsModal').modal()", 300);
                                    $.validator.unobtrusive.parse('#userDetailsModal');
                                }
                            });
                        }
                    }
                },
            ],
            "columns": [
            { "data": "id" },
            { "data": "userFirstName", "class": "wrap-word-td" },
            { "data": "userSurname", "class": "wrap-word-td" },
            { "data": "fullName", "class": "wrap-word-td" },
            { "data": "emailAdress", "class": "wrap-word-td" },
            { "data": "phoneNumber" },
            { "data": "lockout" }
            ],
            "columnDefs": [
                {
                    "targets": 6,
                    "data": "Lockout",
                    "type": "string",
                    "render": function (data, type, full, meta) {
                        var check = "";
                        if (data === true)
                            check = "checked";
                        return "<input class='user-lockout' type='checkbox' " + check + "/>";
                    },
                    "sort": "Lockout"
                }
            ]
        });
    }).ajaxComplete(function () {
    });

}(window.jQuery));


function editUserCallback(data) {
    $('#userDetailsModal').modal('hide');
    $('#admin-users').DataTable().row({ selected: true }).data(data).draw('page');
}

function userActions() {
    $('.user-lockout').on('click', function () {
        var check = $(this).prop('checked');
        var data;
        if (check) {
            data = "True";
        } else {
            data = "False";
        }
        var row = $('#admin-users').DataTable().row($(this).parents('tr')).data();
        var postdata = JSON.stringify({ user: row, isLock: data });
        $.ajax({
            type: 'POST',
            url: '/users/lockout',
            data: postdata,
            contentType: "application/json; charset=UTF-8",
            traditional: true,
            async: false,
            success: function (data) {

            }
        });
    });
}

function editDiscount(link) {
    var val = $(link).text();
    var id = $(link).prev().val();
    var form = $(link).closest('form');
    form.find(".user-discount-btn span").text(val);
    form.find('#CafeId').val(id);
    form.find('#CafeName').val(val);
}