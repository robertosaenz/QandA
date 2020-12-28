import React, { lazy, Suspense } from 'react';
import { BrowserRouter, Route, Redirect, Switch } from 'react-router-dom';
//Redux
import { Provider } from 'react-redux';
import { configureStore } from './redux/Store';

/** @jsx jsx */
import { css, jsx } from '@emotion/core';
import { fontFamily, fontSize, gray2 } from './css/Styles';

//Extra Components
//import Header from './components/Header'; WITHOUT PARAMS ROUTER
import { HeaderWithRouter as Header } from './components/Header'; //PARAMS ROUTER

//Pages
import HomePage from './components/HomePage';
// import AskPage from './components/AskPage';
import SearchPage from './components/SearchPage';
import SignInPage from './components/SignInPage';
import NotFoundPage from './components/NotFoundPage';
import QuestionPage from './components/QuestionPage';
const AskPage = lazy(() => import('./components/AskPage'));

const store = configureStore();

const App: React.FC = () => {
  // const App = () => {
  return (
    <Provider store={store}>
      <BrowserRouter>
        <div
          css={css`
            font-family: ${fontFamily};
            font-size: ${fontSize};
            color: ${gray2};
          `}
        >
          <Header />
          <Switch>
            <Redirect from="/home" to="/" />
            <Route exact path="/" component={HomePage} />
            <Route path="/search" component={SearchPage} />
            <Route path="/ask">
              <Suspense
                fallback={
                  <div
                    css={css`
                      margin-top: 100px;
                      text-align: center;
                    `}
                  >
                    Loading...
                  </div>
                }
              >
                <AskPage />
              </Suspense>
            </Route>
            <Route path="/signin" component={SignInPage} />
            <Route path="/questions/:questionId" component={QuestionPage} />
            <Route component={NotFoundPage} />
          </Switch>
        </div>
      </BrowserRouter>
    </Provider>
  );
};
export default App;
