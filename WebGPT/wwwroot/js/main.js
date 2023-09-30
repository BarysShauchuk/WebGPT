function scrollToBottom() {
    var messages = document.getElementById("messages");
    var element = document.getElementById("last-qa");

    if (messages && element) {
        messages.scrollTop = element.offsetTop - messages.offsetTop;
    }
}
