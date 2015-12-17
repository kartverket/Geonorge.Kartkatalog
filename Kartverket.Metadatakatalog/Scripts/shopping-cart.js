﻿function updateShoppingCart() {
    var shoppingCartElement = $('#orderitem-count');
    var orderItems = "";
    var orderItemsObj = {};
    var cookieName = "orderitems";
    var cookieValue = 0;
    var cookieDomain = ".geonorge.no";

    if (localStorage.getItem("orderItems") != null && localStorage.getItem("orderItems") != "[]") {
        orderItems = localStorage.getItem("orderItems");
    }

    if (orderItems != "") {
        shoppingCartElement.css("display", "block");
        orderItemsObj = JSON.parse(orderItems);
        cookieValue = orderItemsObj.length;
        shoppingCartElement.html(cookieValue);
    } else if ($.cookie(cookieName) != 0 && $.cookie(cookieName) != null) {
        cookieValue = $.cookie(cookieName);
        shoppingCartElement.css("display", "block");
        shoppingCartElement.html(cookieValue);
    } else {
        shoppingCartElement.css("display", "none");
    }
    $.cookie(cookieName, cookieValue, { expires: 7, path: '/', domain: cookieDomain });
}

function updateShoppingCartCookie() {
    var cookieValue = 0;
    if (localStorage.getItem("orderItems") != null && localStorage.getItem("orderItems") != "[]") {
        var orderItems = localStorage.getItem("orderItems");
        var orderItemsObj = JSON.parse(orderItems);
        var cookieValue = orderItemsObj.length;
    } else {
        cookieValue = 0;
    }
    $.cookie(cookieName, cookieValue, { expires: 7, path: '/', domain: cookieDomain });
}


$(window).load(function () {
    updateShoppingCart();
});