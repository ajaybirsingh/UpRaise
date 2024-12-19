import { CampaignTypes, CampaignStatuses } from '../../../../shared/enums';

export interface IYourCampaignRequestDTO {

    pageNumber: number;
    pageSize: number;

    sortOrder: YourCampaignSortOrders;
}


export enum YourCampaignSortOrders {
    Newest = 0,
    Oldest = 1,
}


export interface IYourCampaignResponseDTO {
    pageNumber: number;
    pageSize: number;

    totalPages: number;
    totalItems: number;

    yourCampaigns: IYourCampaignDataDTO[];
}


export interface IYourCampaignDataDTO {
    campaignId: number;

    transactionId: string;

    status: CampaignStatuses;
    type: CampaignTypes;

    headerPictureURL: string;
    name: string;
    description: string;

    startDate: Date;
    endDate: Date;

    createdAt: Date;
    updatedAt: Date;
}
