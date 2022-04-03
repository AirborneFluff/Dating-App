import { Photo } from "./photo";

export interface Member {
    id: number;
    username: string;
    photoUrl: string;
    dateOfBirth: Date;
    created: Date;
    lastActive: Date;
    age: number;
    knownAs: string;
    gender: string;
    introduction: string;
    lookingFor: string;
    interests: string;
    city: string;
    country: string;
    photos: Photo[];
}

