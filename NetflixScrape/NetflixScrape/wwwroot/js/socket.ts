namespace SocketDemo {
    const socketUrl = `ws://${location.host}/ws-source`;

    function openSocket(listener: EventListenerOrEventListenerObject) {
        var socket = new WebSocket(socketUrl);
        socket.addEventListener('message', listener, false);
        return socket;
    }


    document.addEventListener('DOMContentLoaded', evt => {
        const form = document.forms[0];
        const messageInput = <HTMLInputElement>document.getElementById("message");
        const received = document.getElementById("received");
        const socket = openSocket((evt: MessageEvent) => {
            let output = document.createElement("li");
            output.textContent = evt.data;
            received.appendChild(output);
        });
        form.onsubmit = evt => {
            socket.send(messageInput.value);
            evt.preventDefault();
        };
    }, false);
}