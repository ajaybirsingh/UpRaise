import { Route, UrlMatchResult, UrlSegment } from '@angular/router';


export function isCampaign(url: UrlSegment[]): UrlMatchResult {
    if (url.length >= 1) {
        if (url[0].path.toLowerCase().startsWith('campaign-')) {
            return { consumed: [url[0]] };
        }
    }

    return null;
}
