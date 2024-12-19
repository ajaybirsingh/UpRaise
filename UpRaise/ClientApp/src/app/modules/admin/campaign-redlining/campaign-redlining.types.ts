export interface ICampaignRedline {
    id: number;
    transactionId: string;
    type: number;
    status: number;

    categoryId: number;
    name: string;

    summary: string;

    createdByUserAliasId: string;
    showProfilePic: string;

    headerPictureURL: string;
    beneficiaryOrganizationName: number;

    location: string;

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

    photos: string[];
    videos: string[];

    comments: ICampaignRedlineComment[];
    events: ICampaignRedlineEvent[];
}



export interface ICampaignRedlineComment {
    campaignId: number;
    userAliasId: string;
    userFullName: string;
    comment: string;
    createdAt: Date;
}


export interface ICampaignRedlineEvent {
    campaignId: number;
    userAliasId: string;
    userFullName: string;
    note: string;
    eventType: number;
    createdAt: Date;
}

export interface ICampaignRedlineCommentRequest {
    campaignId: number;
    comment: string;
}

export interface ICampaignRedlineStatusDTO {
    transactionId: string;
    approved: boolean;
}
