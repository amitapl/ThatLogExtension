var storyControllers = angular.module('storyControllers', []);

var severity = ['Unspecified', 'Debug', 'Info', 'Warning', 'Error', 'Critical'];

var severityColor = ['default', 'default', 'info', 'warning', 'danger', 'danger'];

function dateFormat(date) {
    return moment(date).format('YYYY-MM-DD HH:mm:ss');
}

function getNames(story) {
    var names = [story.name];

    if (story.children) {
        for (var i = 0; i < story.children.length; i++) {
            names.push(getNames(story.children[i]));
        }
    }

    return names;
}

function getLogs(story) {
    var logs = story.log;
    if (logs) {
        logs.forEach(function (log) {
            log.dateTime = Date.parse(log.dateTime);
        });
    }

    if (logs && story.children) {
        for (var i = 0; i < story.children.length; i++) {
            logs = getLogs(story.children[i]).reduce(function (coll, item) {
                coll.push(item);
                return coll;
            }, logs);
        }
    }

    return logs;
}

function getData(story) {
    var data = story.data;

    if (data && story.children) {
        for (var i = 0; i < story.children.length; i++) {
            data = getData(story.children[i]).reduce(function (coll, item) {
                coll.push(item);
                return coll;
            }, data);
        }
    }

    return data;
}

function join(names, joiner) {;
    var output;

    if (names.length > 0) {
        output = names[0];
    } else {
        output = '';
    }

    for (var i = 1; i < names.length; i++) {
        output += joiner + names[i];
    }

    return output;
}

storyControllers.controller('StoriesCtrl', function ($scope, $state, $stateParams, $filter, ngTableParams, stories) {
    var change = false;

    var daterange = {};
    daterange.startDate = $stateParams.startDate ? moment($stateParams.startDate) : moment().minute(0).hour(0);
    daterange.endDate = $stateParams.endDate ? moment($stateParams.endDate) : moment().add('hours', 1).minute(0);

    $scope.daterange = daterange;

    $scope.daterangeTitle = moment(daterange.startDate).format('MMMM D, YYYY LT') + ' - ' + moment(daterange.endDate).format('MMMM D, YYYY LT');

    $scope.ranges = {
        'Past Hour': [moment().minute(0), moment().add('hours', 1).minute(0)],
        'Last 6 Hours': [moment().minute(0).subtract('hours', 5), moment().add('hours', 1).minute(0)],
        'Last 12 Hours': [moment().minute(0).subtract('hours', 11), moment().add('hours', 1).minute(0)],
        'Today': [moment().minute(0).hour(0), moment().add('hours', 1).minute(0)],
        'Yesterday': [moment().subtract('days', 2).minute(0).hour(0), moment().subtract('days', 1).minute(0).hour(0)],
        'Last 7 Days': [moment().subtract('days', 6).minute(0).hour(0), moment().minute(0)],
        'Last 30 Days': [moment().subtract('days', 29).minute(0).hour(0), moment().minute(0)]
    };

    $scope.$watch("daterange", function (newValue, oldValue) {
        if (change) {
            $state.go('list-stories', { startDate: newValue.startDate.format(), endDate: newValue.endDate.format() });
        }

        change = true;
    });

    $scope.toggle = function (scope) {
        scope.toggle();
    };

    $scope.openStory = function (story) {
        console.log(story.instanceId);
    }

    stories.list(daterange, function (stories) {
        var items = stories.Items.map(function (story) {
            var item = {
                date: dateFormat(story.StartDateTime),
                name: story.Name,
                instanceId: story.InstanceId,
                shortInstanceId: story.InstanceId.substr(0, 6),
                story: story.Json ? JSON.parse(story.Json) : null
            };

            item.children = item.story ? item.story.children : null;
            item.names = getNames(item.story);
            item.namesStr = join(item.names, ' / ');
            item.logs = getLogs(item.story);
            item.data = getData(item.story);
            if (item.story) {
                item.startDateTime = Date.parse(item.story.startDateTime);
            }

            return item;
        });

        $scope.stories = items;

        $scope.tableParams = new ngTableParams({
            page: 1,            // show first page
            count: 10,          // count per page
            sorting: {
                date: 'asc'     // initial sorting
            }
        }, {
            getData: function ($defer, params) {
                var filteredData = $filter('filter')(items, $scope.filter);
                var orderedData = params.sorting() ?
                                    $filter('orderBy')(filteredData, params.orderBy()) :
                                    filteredData;

                $defer.resolve(orderedData.slice((params.page() - 1) * params.count(), params.page() * params.count()));
            },
            $scope: $scope
        });

        var currentPage = null;
        $scope.$watch("filter.$", function (newValue) {
            $scope.tableParams.reload();
            $scope.tableParams.page(1); //Add this to go to the first page in the new pagging

            /*if (newValue && newValue.length > 0) {
                if (currentPage === null) {
                    currentPage = $scope.tableParams.$params.page;
                }
                $scope.tableParams.page(1);
            } else {
                $scope.tableParams.page(currentPage);
                currentPage = null;
            }*/
        });
    });
});

storyControllers.controller('StoryCtrl', function ($scope, $stateParams) {
    $scope.story = $stateParams.story;

    $scope.color = function (sev) {
        if (sev) {
            return severityColor[sev];
        }

        return 'default';
    }

    $scope.text = function (sev) {
        if (sev) {
            return severity[sev];
        }

        return 'Unspecified';
    }
});

var storyApp = angular.module('viewstory', ['ui.router', 'ngJsonExplorer', 'ui.tree', 'ngTable', 'ngBootstrap', 'storyControllers']);

storyApp.config(function ($stateProvider, $urlRouterProvider) {
    $stateProvider.
        state('list-stories', {
            url: '/?startDate&endDate',
            templateUrl: 'stories.html',
            controller: 'StoriesCtrl'
        }).
    state('show-story', {
        url: '/story',
        templateUrl: 'story.html',
        controller: 'StoryCtrl',
        params: {
            story: null
        }
    });

    $urlRouterProvider.otherwise('/');
});

storyApp.factory('stories', function ($http) {
    return {
        list: function (daterange, callback) {
            $http({
                method: 'GET',
                url: 'api/storytable?from=' + daterange.startDate.format() + '&to=' + daterange.endDate.format(),
                cache: true
            }).success(callback);
        }
    };
});

/*countryApp.directive('country', function () {
    return {
        scope: {
            country: '='
        },
        restrict: 'A',
        templateUrl: 'country.html',
        controller: function ($scope, countries) {
            countries.find($scope.country.id, function (country) {
                $scope.flagURL = country.flagURL;
            });
        }
    };
});*/
