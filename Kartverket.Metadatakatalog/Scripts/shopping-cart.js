function updateShoppingCart() {
    var shoppingCartElement = $('#orderitem-count');
    var orderItems = "";
    if (localStorage.getItem("orderItems") != null && localStorage.getItem("orderItems") != "[]") {
        orderItems = localStorage.getItem("orderItems");
    } else {
        orderItems = "";
    }
    var orderItemsObj = {};
    var cookieName = "orderitems";
    var cookieValue = 0;
    var cookieDomain = ".geonorge.no";

    if (orderItems == "") {
        shoppingCartElement.css("display", "none");
    } else {
        shoppingCartElement.css("display", "block");
        orderItemsObj = JSON.parse(orderItems);
        cookieValue = orderItemsObj.length;
        shoppingCartElement.html(cookieValue);
    } 
    $.cookie(cookieName, cookieValue, { expires: 7, path: '/', domain: cookieDomain });
}


$(window).load(function () {
    updateShoppingCart();
});