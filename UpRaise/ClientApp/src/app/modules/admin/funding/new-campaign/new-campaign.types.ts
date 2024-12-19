import { CampaignTypes } from "../../../../shared/enums";

export enum WizardEventTypes {
    Next = 1,
    Previous = 2,
    GoLive = 3,
    SaveDraft = 4,
}


export enum FileUploadStates {
    Added = 1,
    Loaded = 2,
    Deleted = 3,
}

export interface IEditCampaignOrganization {
    categoryId: number;

    beneficiaryOrganization: string,
    location: string,
    contactName: string,
    contactEmail: string,
    contactPhone: string,

    campaignConditions: string[],

    beneficiaryMessage: string,
}

export interface IEditCampaignPeople {
    beneficiaryName: string,
    beneficiaryEmail: string,
    beneficiaryMessage: string,
}

export interface IEditCampaign {
    id: number;
    transactionId: string,
    typeId: CampaignTypes;

    geoLocationCountryId: number,
    geoLocationAddress: string,

    organization: IEditCampaignOrganization;
    people: IEditCampaignPeople;

    fundraisingGoals?: number,
    currencyId: number,
    startDate?: Date,
    endDate?: Date,
    headerPhoto: IEditCampaignFile,
    photos: IEditCampaignFile[],
    videos: IEditCampaignFile[],

    acceptTermsAndConditions: boolean,

    name: string;
    description: string;

    distributionTerms: string[],

}

export interface IEditCampaignFile {
    id: number;
    url: string;
}



export interface ICampaignPreview {
    type: number;
    category: string;

    name: string;
    description: string; 

    headerPhoto: any;

    beneficiaryOrganization: string,
    location: string,
    contactName: string,
}


export interface IGeocodingResponse {
    formattedAddress: string;
    latitude: number;
    longitude: number;
}

export interface IGeocodingRequest {
    //countryId: number;
    location: string;
}
