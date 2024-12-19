
export class ResultDTO {
  status: StatusEnum;
  message: string;
  data: any;
}


export enum StatusEnum {
  Success = 1,
  Error = 2
}
