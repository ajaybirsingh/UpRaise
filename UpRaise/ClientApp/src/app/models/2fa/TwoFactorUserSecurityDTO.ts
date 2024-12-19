export class TwoFactorUserSecurityDTO {
    email: string;
    emailConfirmed: boolean;
    phoneNumber: string;
    externalLogins: string[];
    twoFactorEnabled: boolean;
    hasAuthenticator: boolean;
    twoFactorClientRemembered: boolean;
    recoveryCodesLeft: number[];
}
