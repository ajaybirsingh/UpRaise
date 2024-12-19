import { CampaignTypes } from "../../../../shared/enums";

export interface IDashboardCampaignsResponseDTO {
    id: number;
    type: number;
    name: string;
}


export interface IDashboardCampaignRequestDTO {
    id: number;
    type: number;
    dateRange: number;
}


export interface IDashboardCampaignResponseDTO {
    id: number;
    type: number;
    name: string;
    location: string;
    beneficiaryName: string;
    startDate: Date;
    endDate?: Date;
    daysLeft?: number;
    numberOfTotalContributors: number;
    numberOfUniqueContributors: number;
    fundraisingGoal?: number;
    amountRaised?: number;
    contributions: IDashboardCampaignContributionDTO[];
}

export interface IDashboardCampaignContributionDTO {
    id: number;
    date: Date;
    amount: number;
    contributionTypeId: number;
}


export interface IDashboardActivityRequestDTO {
    campaignId: number;
}


export interface IDashboardActivityResponseDTO {
    id: number;
    icon?: string;
    userAliasId?: string;
    image?: string;
    description?: string;
    date: string;
    extraContent?: string;
    linkedContent?: string;
    link?: string;
    useRouter?: boolean;
}


export interface IEmailBeneficiaryRequestDTO {
    campaignId: number;
}
