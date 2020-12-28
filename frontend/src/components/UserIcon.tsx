import React from 'react';

/** @jsx jsx */
import { css, jsx } from '@emotion/core';

//Resources
import userlogo from '../user.svg';

const UserIcon = () => (
  <img
    css={css`
      width: 12px;
      opacity: 0.6;
    `}
    src={userlogo}
    alt="User"
    width="12px"
  />
);

export default UserIcon;
