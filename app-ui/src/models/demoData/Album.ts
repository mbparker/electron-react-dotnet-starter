import { NamedEntityWithImage } from "./NamedEntityWithImage";
import {Artist} from "./Artist";
import {Track} from "./Track";
import {LazyShim} from "../LazyShim";

export class Album extends NamedEntityWithImage {
    public artistId: number = 0;
    public releaseDate: Date = new Date();
    public artist: LazyShim<Artist> = new LazyShim<Artist>();
    public tracks: LazyShim<Track[]> = new LazyShim<Track[]>();
}
