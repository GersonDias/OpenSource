/// <reference path='../_all.ts' />

module todos {
    'use strict';

    export class todoItem {
        constructor(
            public title: string,
            public completed: boolean,
            public workItemId: number
        ) {}
    }
}