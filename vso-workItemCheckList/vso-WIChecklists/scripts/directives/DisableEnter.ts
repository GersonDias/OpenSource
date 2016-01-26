
module todos {
    export function disableEnterSubmit(): ng.IDirective {
        return {
            link: ($scope: ng.IScope, element: JQuery, attributes: any) => {
                element.bind('keydown', (event) => {
                    if (event.keyCode == 13) {
                        event.preventDefault();

                        return false;
                    };
                });

                $scope.$on('$destroy', () => { element.unbind('keydown'); });
            }
        }
    }
};