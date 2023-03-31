function scrollIntoView(elementId) {
    var elem = document.getElementById(elementId);
    if (elem) {
        elem.scrollIntoView({block:"center"});
        window.location.hash = elementId;
    }
}

function scrollToTop() {
    window.scrollTo(0,0);
}