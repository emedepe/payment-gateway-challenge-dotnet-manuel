# Challenge documentation

## Code repository

- I have left the completed exercise as a PR for you to review, thank you. I thought this would be a good way to show my work and compare it easily with the original solution. For this work I have not committed as granularly as I would normally, but instead preferred to highlight whole pieces of functionality; I thought it might be easier for you to follow my work this way.

- I was a bit confused about whether I should fork your repo and create a PR against it or not, the email asked to fork, but the guidelines said no PR, so I chose to mimic it without forking, I hope that is OK. Please do not hesitate to ask if you prefer me to fork your repo and raise a PR against it or to zip you the code.

## Architecture

- I decided to go for a simple repository pattern after considering using `MediatR`. The guidelines recommended going for simplicity, and I think the repository pattern is good enough to demonstrate the exercise following good practices without extra complication.

## Considerations I have taken

- I have changed the property `CardNumberLastFour` to `CardNumber` in `PostPaymentRequest`, since I understand that in this flow the API should receive the whole card number.  I also changed the type to `string` to prevent the size limitations of `int` and `long`, and also because a card number is static data that will not be used for arithmetic operations.

- I added JSON property names to the transfer objects to conform with the established JSON syntax. I used data annotations manually because configuring JSON properties generically often leads to misunderstandings. Data annotations are easier to read at a glance, and sometimes we may want to change the property name entirely, not just the casing.

- When creating the transfer objects for communicating with the bank, I used classes for consistency with the rest of the solution, although a record could also be a good choice.

- For simplicity, I mapped classes directly where needed for the exercise. It could be worth considering a mapping service or using a library like `AutoMapper` in a more complete solution.

- I create an HTTP client for communication with the bank at start up, since it is advisable to let the built-in client factory handle underlying connections; I also configure a retry mechanism in there, so it is done just once. The client could be injected directly in the `PaymentsProcessingService` service (or another bank custom service). I decided to wrap it in a custom client factory, for easy testing and to allow for a further degree of decoupling.

- I opted to return a `400 BadRequest` if the payment request is `Rejected`, clearly specifying the errors. I still include a `Status` field with the correct value in the body of the error response. For errors coming from the bank, I decided to propagate the `503 ServiceUnavailable` and to mask generic exceptions with a `502 BadGateway`. In both cases the raw error is not propagated, and the client receives a custom one with meaningful messages.

- I created an interface for `PaymentsRepository` because it is good practice. Also to highlight that a real data source should probably be scoped, although in this case it must be singleton for testing purposes. I also changed the internal data structure to be thread-safe, and made the return type for the `Get` method nullable.

- I modified the existing controller method `GetPaymentAsync` to return a `404 NotFound` when no transaction with the given `Id` exists in the repository. This method does not need to be asynchronous in this example, but I left it as it is since it would be with a real data source.

- I updated the existing controller tests to reflect the new repository changes and renamed them to allow for the new method and clarify where they apply. In the tests, I do not check for required values on properties that are integers, since a missing or null value in the payload will default to zero.