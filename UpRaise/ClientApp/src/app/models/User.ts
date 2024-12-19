export class User {
    id: number;
    aliasId: string;
    companyId: number;
    username: string;
    password: string;
    firstName: string;
    lastName: string;
    token: string;
    pictureURL: string;

    //companyName: string;
    //companyPictureURL: string;

    updatedAt: string;
}


export interface IUser {
    id: string;
    aliasId: string;
    companyId: number;
    username: string;
    firstName: string;
    lastName: string;
    pictureURL: string;
    //companyName: string;
    //companyPictureURL: string;
    updatedAt?: string;
}


export interface IUserNotifications {
    notificationOnCampaignDonations: boolean;
    notificationOnCampaignFollows: boolean;
    notificationOnUpraiseEvents: boolean;
}

export interface IUserPersonalInformation {
    firstName: string;
    lastName: string;
    country: string;
    streetAddress: string;
    city: string;
    stateProvince: string;
    zipPostal: string;
    defaultCurrencyId: number;
}


