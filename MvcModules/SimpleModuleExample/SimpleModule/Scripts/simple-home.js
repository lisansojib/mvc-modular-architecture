$(function () {
    $.get("/simplemodule/api/values").done(function (data) { console.log(data) });
})