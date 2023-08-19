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

async function downloadFileFromStream(fileName, contentStreamReference) {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
}