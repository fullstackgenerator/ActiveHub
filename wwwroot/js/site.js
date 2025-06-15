// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
//clear TempData messages after a certain time

//purchase-related
$(document).ready(function() {
    setTimeout(function() {
        $(".alert").fadeOut("slow");
    }, 5000);
});

$(document).ready(function () {
    $('.datepicker').datepicker({
        format: 'dd.mm.yyyy',
        weekStart: 1,
        autoclose: true,
        todayHighlight: true
    });

    //clear button functionality
    $('#btnClearFilter').on('click', function() {
        //clear date inputs
        $('input[name="fromDate"]').val('');
        $('input[name="toDate"]').val('');
        $('#filterForm').submit();
    });
});