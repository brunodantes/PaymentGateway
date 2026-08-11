# PaymentGateway

`PaymentGateway` is a simple, modular **Payment Gateway** built with **.NET**, designed to:

- Receive and validate payment requests  
- Call an external **Bank Simulator** service  
- Apply **retry policies** with Polly  
- Persist payment data in-memory  
- Expose endpoints to **create** a payment and **query** its status

The project is ideal for learning, testing, and demonstrating patterns such as validation, external HTTP calls, resilience, and basic domain separation.

---

## Solution Structure

The solution is split into multiple projects:

- **PaymentGateway.API** – ASP.NET Core Web API, controllers, DI configuration, Serilog, Swagger  
- **PaymentGateway.Application** – Application services and validators  
- **PaymentGateway.Contract** – Request/response contracts and enums  
- **PaymentGateway.Domain** – Domain interfaces  
- **PaymentGateway.Entities** – Domain entities  
- **PaymentGateway.Infrastructure** – Bank Simulator integration (Refit + Polly)  
- **PaymentGateway.Data** – In-memory repository implementation  
- **PaymentGateway.Application.Tests** – Unit tests and fakes  

---

## API

### **POST /payment**

Creates a new payment request, validates the data, triggers bank authorization, and stores the result.

#### Request Example

```json
{
  "card_number": "4111111111111111",
  "expiry_month": 12,
  "expiry_year": 2028,
  "currency": "USD",
  "amount": 150.00,
  "CVV": "123"
}
```

#### Response Example

```json
{
  "id": "9b2e8a06-3ea3-4af4-b3c5-21e9c1f0f0a4",
  "paymentStatusEnum": "Authorized",
  "expiry_month": 12,
  "expiry_year": 2028,
  "currency": "USD",
  "amount": 150.00,
  "card_number": "1111"
}
```

---

### **GET /payment?paymentId={GUID}**

Retrieves the status and details of a processed payment.

#### Example

```
GET /payment?paymentId=9b2e8a06-3ea3-4af4-b3c5-21e9c1f0f0a4
```

#### Response

```json
{
  "id": "9b2e8a06-3ea3-4af4-b3c5-21e9c1f0f0a4",
  "paymentStatusEnum": "Authorized",
  "expiry_month": 12,
  "expiry_year": 2028,
  "currency": "USD",
  "amount": 150.00,
  "card_number": "1111"
}
```

---

## Bank Simulator

The service uses **Refit** to communicate with an external bank simulator:

```csharp
[Post("/payments")]
Task<ApiResponse<BankResponseModel>> Publish(BankRequestModel bankRequestModel);
```

### Polly Retry Policy

```csharp
_retryPolicy = Policy
    .Handle<Exception>()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt =>
        {
            var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
            var jitter = TimeSpan.FromMilliseconds(new Random().Next(0, 1000));
            return baseDelay + jitter;
        },
        onRetry: OnRetry);
```

If the simulator fails (timeout, 500, null body), Polly retries up to 3 times.

---

##️ In‑Memory Repository

`PaymentRepository` stores payments in a simple list:

```csharp
public Task AddPayment(PaymentModel paymentRequest) =>
    Task.FromResult(_payments.Add(paymentRequest));

public Task<PaymentModel?> GetPayment(Guid id) =>
    Task.FromResult(_payments.FirstOrDefault(x => x.Id == id));
```

Registered as a **singleton**, so data resets when the application restarts.

---

## How to Run

```
dotnet restore
dotnet build
cd PaymentGateway.API
dotnet run
```

Swagger UI is enabled in Development environment.

---

## Technologies Used

- ASP.NET Core Web API  
- C#  
- Refit  
- Polly  
- Serilog  
- FluentValidation  
- Swagger / OpenAPI  
