import { CampaignTypes } from "../../../../shared/enums";
import { IPublicCampaignDetailContribution } from "../home.types";

export interface IPublicCampaignContributionResponseDTO {
    pageNumber: number;
    pageSize: number;

    totalNumberOfContributions: number;
    totalPages: number;

    contributions: IPublicCampaignDetailContribution[];
}


export interface IPublicCampaignContributionRequestDTO {

    campaignId: number;
    campaignType: CampaignTypes;

    pageNumber: number;
    pageSize: number;

    sortField: ContributionSortFields;
    sortDirection: SortDirections;
}


export enum ContributionSortFields {
    Date = 1,
    TopContributions = 2,
}


export enum SortDirections {
    Ascending = 1,
    Descending = 2,
}
