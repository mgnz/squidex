/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';

import {
    AssetsState,
    Queries,
    UIState
} from '@app/shared';

@Component({
    selector: 'sqx-assets-filters-page',
    styleUrls: ['./assets-filters-page.component.scss'],
    templateUrl: './assets-filters-page.component.html'
})
export class AssetsFiltersPageComponent {
    public queries = new Queries(this.uiState, 'assets');

    constructor(
        public readonly assetsState: AssetsState,
        private readonly uiState: UIState
    ) {
    }

    public search(query: string) {
        this.assetsState.search(query);
    }

    public selectTags(tags: string[]) {
        this.assetsState.selectTags(tags);
    }

    public toggleTag(tag: string) {
        this.assetsState.toggleTag(tag);
    }

    public resetTags() {
        this.assetsState.resetTags();
    }

    public isSelectedQuery(query: string) {
        return query === this.assetsState.snapshot.assetsQuery || (!query && !this.assetsState.assetsQuery);
    }

    public trackByTag(index: number, tag: { name: string }) {
        return tag.name;
    }

    public trackByQuery(index: number, query: { name: string }) {
        return query.name;
    }
}