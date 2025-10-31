import { NamedEntityWithImage } from "./NamedEntityWithImage";
import {Album} from "./Album";
import {LazyShim} from "../LazyShim";

export class Artist extends NamedEntityWithImage {
    public albums: LazyShim<Album[]> = new LazyShim<Album[]>();
}
