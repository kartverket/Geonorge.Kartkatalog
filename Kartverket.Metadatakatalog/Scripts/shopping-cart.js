function updateShoppingCart() {
    var shoppingCartElement = $('#orderitem-count');
    var orderItems = "";
    var orderItemsObj = {};
    var cookieName = "orderitems";
    var cookieValue = 0;
    var cookieDomain = ".geonorge.no";


    if (localStorage.getItem("orderItems") != null && localStorage.getItem("orderItems") != "[]") {
        orderItems = localStorage.getItem("orderItems");
    } else {
        orderItems = "";
    }

    if (orderItems != "") {
        shoppingCartElement.css("display", "block");
        orderItemsObj = JSON.parse(orderItems);
        cookieValue = orderItemsObj.length;
        shoppingCartElement.html(cookieValue);

    } else if ($.cookie(cookieName)) {
        cookieValue = $.cookie(cookieName);
        shoppingCartElement.css("display", "block");
        shoppingCartElement.html(cookieValue);
    } else {
        shoppingCartElement.css("display", "none");
    }
    $.cookie(cookieName, cookieValue, { expires: 7, path: '/', domain: cookieDomain });
}


$(window).load(function () {
    updateShoppingCart();
});