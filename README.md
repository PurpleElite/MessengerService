A simple service for sending and retrieving messages. Messages are stored in a pre-populated in-memory database and contain the address of the recipient and sender, a plaintext string of text, a timestampe of when the message was sent, and a timestamp of when the message was marked as read. All interactions with the database are done through a Rest API.

## Endpoints:
**GET: api/Messages/{start:datetime?}/{end:datetime?}**\
Displays all messages with the option to only grab messages before and/or after given dates.

**GET: api/Messages/{id:GUID}**\
Retrieves a single message with the given ID.

**GET: api/Messages/recipients**\
Retrieves all user adresses that have received messages.

**GET: api/Messages/recipients/{userAddress:string}/{start:datetime?}/{end:datetime?}**\
Retrieves all messages addressed to a given recipient, with the option to filter by date range.

**GET: api/Messages/recipients/{userAddress:string}/unread/{start:datetime?}/{end:datetime?}**\
Retrieves all unread messages addressed to a given recipient, with the option to filter by date range.

**PATCH: api/Messages/mark-read**\
Takes an array of GUIDs as input and marks all the corresponding messages as read. Returns 404 NotFound if none of the GUIDs map to existing messages, otherwise returns a list of GUIDs that were found.

**PATCH: api/Messages/mark-unread**\
The same thing as mark-read, but marking as unread instead.

**POST: api/Messages**\
Used to send messages. Accepts three string parameters: content, recipientAddress, and senderAddress. Returns a BadRequest response if recipient and sender addresses are not valid email addresses.

**DELETE: api/Messages/{id:Guid}**\
Deletes a single message with the given GUID.

**DELETE: api/Messages**\
Takes an array of GUIDs as input and deletes the corresponding messages. If none of the IDs can be found returns a 404 Not Found error, otherwise returns a list of all Message IDs that were deleted.
