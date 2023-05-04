export interface User{
    username:string;
    token:string;
    photoUrl: string;
    knownAs: string;
    gender: string;
    roles: string[];
}
//different from Member:
//member is what they want to show to the audience : interest, age, ...
//user is their credential, saved in the browser.

//different than interface in c#, we down't neet to prefix a I