import {NamedEntity} from "./NamedEntity";
import {Genre} from "./Genre";
import {Artist} from "./Artist";
import {Album} from "./Album";
import {LazyShim} from "../LazyShim";

export class Track extends NamedEntity {
    public genreId: number | undefined;
    public artistId: number = 0;
    public albumId: number = 0;
    public rating: number = 0.0;
    public dateAdded: Date = new Date();
    public discNumber: number | undefined;
    public trackNumber: number | undefined;
    public duration: number = 0.0;
    public filename: string = '';
    public genre: LazyShim<Genre> = new LazyShim<Genre>();
    public artist: LazyShim<Artist> = new LazyShim<Artist>();
    public album: LazyShim<Album> = new LazyShim<Album>();
}
