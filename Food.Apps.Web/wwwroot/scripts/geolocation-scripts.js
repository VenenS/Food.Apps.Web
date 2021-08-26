(function ($) {
    ymaps.ready(function () {
        $.ajax({
            url: '/City/IsCityConfirmed',
            success: function (isConfirmed) {
                if (!isConfirmed) {
                    $.ajax({
                        method: 'Post',
                        url: '/city/CheckCityAvailable',
                        data: {
                            city: ymaps.geolocation.city,
                            region: ymaps.geolocation.region,
                        },
                        success: function (res) {
                            if (res) {
                                confirmCity(res);
                            }
                        }
                    });
                }
            }
        });
    });
}(window.jQuery));

function confirmCity(res) {
    $('#nav-city').replaceWith(res);
    location.reload();
}

function choiceCity() {
    $('#choiceCityModal').modal('show');
    filterCity();
}

function filterCity() {
    $.ajax({
        method: 'Post',
        url: "/city/filter",
        data: {
            searchString: $('#city-search-string').val()
        },
        success: function (res) {
            $('#cities').replaceWith(res);
        }
    })
}