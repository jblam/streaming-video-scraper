# Architecture

System to be composed of four parts:

1. Socket **client**
2. Socket **server**
3. Extension **background**
4. Extension **content**

## Socket client

Provides the user experience

- Represents the content-state to the user
- Submits commands generated from user input
- Manages concurrency resulting from user input (e.g. touch inputs generated during transmission and processing latency)

The client lifecycle is managed by the user and the host platform (e.g. Android)

## Socket server

Single public host for the service

- Holds active socket connections to clients
- Represents extension background availability
- Broadcasts changes to content-state
- Manages concurrency resulting from multiple clients
- Passes-through commands to extension background
- Passes-back command result to issuing client

The server lifecycle is managed by the host media system platform, e.g. through cron / systemd

## Extension background

Observes connection and history-state changes from content

- Holds web-extension port to relevant open browser tabs
- Prompts re-evaluation of state when content tab appears to change location
- Passes-through commands to content tab
- Passes-back command result to socket server

The background lifecycle is managed by the browser, although the background process may choose to close its server port if no content tabs are available

## Extension content

Interprets the visible state and executes commands

- Establishes connection to web-extension port
- Provides page state model on request
- Executes commands and provides response

The lifecycle is the lifecycle of a browser tab
