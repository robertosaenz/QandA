import React, { FC, useState, useEffect } from 'react';
import { RouteComponentProps } from 'react-router-dom';

/** @jsx jsx */
import { css, jsx } from '@emotion/core';

// Interfaces
import { QuestionsData, searchQuestions } from '../interfaces/QuestionsData';

// Extra Components
import Page from './Page';
import QuestionList from './QuestionList';

const SearchPage: FC<RouteComponentProps> = ({ location }) => {
  const [questions, setQuestions] = useState<QuestionsData[]>([]);

  const searchParams = new URLSearchParams(location.search);
  const search = searchParams.get('criteria') || '';

  useEffect(() => {
    const doSearch = async (criteria: string) => {
      const foundResults = await searchQuestions(criteria);
      setQuestions(foundResults);
    };
    doSearch(search);
  }, [search]);

  return (
    <Page title="Search Results">
      {search && (
        <p
          css={css`
            font-size: 16px;
            font-style: italic;
            margin-top: 0px;
          `}
        >
          for "{search}"
        </p>
      )}
      <QuestionList QuestionsData={questions} />
    </Page>
  );
};

export default SearchPage;
