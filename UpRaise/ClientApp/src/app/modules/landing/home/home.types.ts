/*
export interface Category
{
    id?: string;
    title?: string;
    slug?: string;
}
*/

export interface IPublicCampaign {
    id: number;
    typeId: number;
    categoryId: number;
    name: string;

    summary: string;

    headerPictureURL: string;
    beneficiaryOrganizationName: number;

    location: string;

    createdAt: Date;
    updatedAt: Date;

    fundraisingGoals?: number;

    featured: boolean;

    completed: boolean;

    organizedBy: string;
    organizedByProfilePictureURL: string;

    category: string;
    beneficiary: string;

    numberOfDonors: number;

    daysLeft?: number;

    fundedPreviously: boolean;

    fundedPercentage: number;

    fundingAmountToDate: number;

    fundingLastDateTime: number;

    seoFriendlyURL: string;

    longitude: number;
    latitude: number;

    photos: string[];
    videos: string[];
}

export interface IPublicCampaignDetail {
    id: number;
    transactionId: string;
    type: number;
    category: string;

    numberOfFollowers: number;
    numberOfVisits: number;
    numberOfShares: number;
    numberOfDonors: number;

    name: string;
    description: string;
    headerPictureURL: string;
    beneficiaryOrganizationName: number;

    location: string;

    createdByUserFullName: string;
    createdByUserAliasId: string;
    showProfilePic: boolean;
    isUserFollowing: boolean;

    numberOfContributions: number;

    createdAt: Date;
    updatedAt: Date;

    fundraisingGoals?: number;

    featured: boolean;

    completed: boolean;

    daysLeft?: number;

    fundedPreviously: boolean;

    fundedPercentage: number;

    fundingAmountToDate: number;

    fundingLastDateTime: number;

    seoFriendlyURL: string;

    photos: string[];
    videos: string[];

    isCurrentSignedInUserCreator: boolean;
}

export interface IPublicCampaignDetailContribution {

    userAliasId: string;

    userName: string;

    date: Date;

    amount: number;

    note: string;

    status: ContributionStatuses;
}

export enum ContributionStatuses{
    RecentDonation= 1,
    TopDonation = 2,
    FirstDonation = 3
}
