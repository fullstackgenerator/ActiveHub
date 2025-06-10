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