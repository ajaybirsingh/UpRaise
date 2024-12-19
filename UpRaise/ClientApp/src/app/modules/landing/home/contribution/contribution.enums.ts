export enum ContributionSteps {
    Amount = 1,
    PaymentMethod = 2,
    Options = 3,
    Confirm = 4,
}

export enum ContributionTypes {
    CreditCard = 1, //Stripe
    Crypto = 2,  //BitPay
    ETransfer = 3,
    GooglePay = 4, //BitPay
}
