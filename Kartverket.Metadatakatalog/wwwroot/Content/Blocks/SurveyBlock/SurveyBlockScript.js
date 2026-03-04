var surveyCookie = {
    name: 'AttendedSurvey',
    value: 'survey_attended',
    domain: '.geonorge.no',
    expireDays: 365,
    path: '/'
}

function setSurveyCookie() {
    Cookies.set(surveyCookie.name, surveyCookie.value, { expires: surveyCookie.expireDays, path: surveyCookie.path, domain: surveyCookie.domain });
}

function surveyCookieExists() {
    return Cookies.get(surveyCookie.name) == surveyCookie.value;
}


$(document).ready(function () {
    $('[survey-accept]').on('click', function () {
        setSurveyCookie();
        close_box();
        var surveyExternalUrl = 'https://surveys.enalyzer.com/?pid=hepn6af5';
        if (surveyExternalUrl.length > 0) {
            window.open(surveyExternalUrl, '_blank');
        }
    });

    $('[survey-decline]').on('click', function () {
        setSurveyCookie();
        close_box();
    });

    if (!surveyCookieExists()) {
        setTimeout(function () {
            $('.backdrop, .survey-block').animate({ 'opacity': '0.80' }, 300, 'linear');
            $('.survey-block').animate({ 'opacity': '1.00' }, 300, 'linear');
            $('.backdrop, .survey-block').css('display', 'block');

            $('.backdrop, .close').click(function () {
                close_box();
            });
        }, 10000); // Timeout 10 seconds
    }
});

function close_box() {
    $('.backdrop, .survey-block').animate({ 'opacity': '0' }, 300, 'linear', function () {
        $('.backdrop, .survey-block').css('display', 'none');
    });
}
