function setCookieValue(cookieName, cookieValue, cookieDomain) {
    var now = new Date();
    var hour = 4;
    now.setTime(now.getTime() + (hour * 60 * 60 * 1000));
    var cookieExpireDate = "expires=" + now.toGMTString();

    console.log("cookieName: " + cookieName);
    console.log("cookieValue: " + cookieValue);
    console.log("cookieDomain: " + cookieDomain);
    console.log("cookieExpireDate: " + cookieExpireDate);
    document.cookie = "test=testverdi";
    document.cookie = cookieName + "=" + cookieValue + "; " + cookieExpireDate + "; domain=." + cookieDomain + ";path=/";
}

function deleteCookieValue(cookieName) {
    var d = new Date();
    d.setTime(d.getTime());
    var cookieExpireDate = "expires=" + d.toString();
    document.cookie = cookieName + "=expired;" + cookieExpireDate;
}

var shoppingCartElement = $('.shopping-cart-container .shopping-cart #orderitem-count');

$(window).load(function () {
    var orderItems = JSON.parse(localStorage.getItem("orderItems"));
    var cookieName = "orderitems";
    var cookieValue = orderItems.length;
    var cookieDomain = "geonorge.no";
      
    $.cookie(cookieName, cookieValue, { expires: 7 });

    //deleteCookieValue(cookieName);
   // setCookieValue(cookieName, cookieValue, cookieDomain);
});