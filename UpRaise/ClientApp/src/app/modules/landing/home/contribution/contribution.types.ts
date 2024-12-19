
export const contributionStepsData = [
    {
        order: 0,
        title: 'Amount',
        subtitle: 'Introducing the library and how it works',
    },
    {
        order: 1,
        title: 'Payment Method',
        subtitle: 'Where to find the sample code and how to access it',
    },
    {
        order: 2,
        title: 'Options',
        subtitle: 'How to create a basic Firebase project and how to run it locally',
    },
    {
        order: 3,
        title: 'Confirm',
        subtitle: 'Setting up the Firebase CLI to access command line tools',
    }
];



export interface IStripeCreateRequestDTO {
    campaignId: number;
    amount: number;
    token: string;
}


export interface IStripeCreateResponseDTO {
    clientSecret: string;
}


export interface ICryptoCreateRequestDTO {
    campaignType: number;
    campaignId: number;
    amount: number;
    comment: string;
}


export interface ICryptoCreateResponseDTO {
    url: string;
}


export interface IContributionRequestDTO {
    contributionType: number;

    campaignType: number;
    campaignId: number;

    amount: number;
    comment: string;
}
