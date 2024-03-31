# IPK Project 1 Changelog

## Implemented functionality

- Arguments parsing
- Custom `stdin` handling to prevent using `async`
- Parsing user input and handling it
- **TCP**
- **UDP** with resending packets after delay and maximum retransmits
- Parsing and handling server messages

## Known limitations:

- Message is considered done processing even when it expects `reply`
