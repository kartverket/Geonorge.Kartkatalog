$(window).load(function () {
    var options = {
        disable_search_threshold: 10,
        search_contains: true
    };
    $(".chosen-select").chosen(options);
    $('[data-toggle="tooltip"]').tooltip();
});